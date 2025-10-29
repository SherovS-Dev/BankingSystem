using Npgsql;
using BankingSystem.API.Interfaces;
using BankingSystem.API.Models;

namespace BankingSystem.API.Repositories;

public class TransactionRepository : ITransactionRepository
{
    private readonly string _connectionString;

    public TransactionRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task<Transaction?> GetByIdAsync(int id)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        await using var command = new NpgsqlCommand(@"
            SELECT transactionid, accountid, transactiontype, amount, currency, transactiondate,
                   description, status, toaccountid, referencenumber, balanceafter, createdby
            FROM transactions 
            WHERE transactionid = @Id", connection);
        
        command.Parameters.AddWithValue("@Id", id);

        await using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return MapTransaction(reader);
        }

        return null;
    }

    public async Task<Transaction?> GetByReferenceNumberAsync(string referenceNumber)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        await using var command = new NpgsqlCommand(@"
            SELECT transactionid, accountid, transactiontype, amount, currency, transactiondate,
                   description, status, toaccountid, referencenumber, balanceafter, createdby
            FROM transactions 
            WHERE referencenumber = @ReferenceNumber", connection);
        
        command.Parameters.AddWithValue("@ReferenceNumber", referenceNumber);

        await using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return MapTransaction(reader);
        }

        return null;
    }

    public async Task<IEnumerable<Transaction>> GetByAccountIdAsync(int accountId, int page = 1, int pageSize = 20)
    {
        var transactions = new List<Transaction>();

        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        await using var command = new NpgsqlCommand(@"
            SELECT transactionid, accountid, transactiontype, amount, currency, transactiondate,
                   description, status, toaccountid, referencenumber, balanceafter, createdby
            FROM transactions 
            WHERE accountid = @AccountId
            ORDER BY transactiondate DESC
            LIMIT @PageSize OFFSET @Offset", connection);
        
        command.Parameters.AddWithValue("@AccountId", accountId);
        command.Parameters.AddWithValue("@Offset", (page - 1) * pageSize);
        command.Parameters.AddWithValue("@PageSize", pageSize);

        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            transactions.Add(MapTransaction(reader));
        }

        return transactions;
    }

    public async Task<IEnumerable<Transaction>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        var transactions = new List<Transaction>();

        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        await using var command = new NpgsqlCommand(@"
            SELECT transactionid, accountid, transactiontype, amount, currency, transactiondate,
                   description, status, toaccountid, referencenumber, balanceafter, createdby
            FROM transactions 
            WHERE transactiondate BETWEEN @StartDate AND @EndDate
            ORDER BY transactiondate DESC", connection);
        
        command.Parameters.AddWithValue("@StartDate", startDate);
        command.Parameters.AddWithValue("@EndDate", endDate);

        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            transactions.Add(MapTransaction(reader));
        }

        return transactions;
    }

    public async Task<Transaction> CreateAsync(Transaction transaction)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        await using var command = new NpgsqlCommand(@"
            INSERT INTO transactions (accountid, transactiontype, amount, currency, transactiondate, 
                                     description, status, toaccountid, referencenumber, balanceafter, createdby)
            VALUES (@AccountId, @TransactionType, @Amount, @Currency, @TransactionDate, 
                    @Description, @Status, @ToAccountId, @ReferenceNumber, @BalanceAfter, @CreatedBy)
            RETURNING transactionid", connection);

        command.Parameters.AddWithValue("@AccountId", transaction.AccountId);
        command.Parameters.AddWithValue("@TransactionType", transaction.TransactionType);
        command.Parameters.AddWithValue("@Amount", transaction.Amount);
        command.Parameters.AddWithValue("@Currency", transaction.Currency);
        command.Parameters.AddWithValue("@TransactionDate", transaction.TransactionDate);
        command.Parameters.AddWithValue("@Description", transaction.Description ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@Status", transaction.Status);
        command.Parameters.AddWithValue("@ToAccountId", transaction.ToAccountId.HasValue ? transaction.ToAccountId.Value : DBNull.Value);
        command.Parameters.AddWithValue("@ReferenceNumber", transaction.ReferenceNumber ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@BalanceAfter", transaction.BalanceAfter.HasValue ? transaction.BalanceAfter.Value : DBNull.Value);
        command.Parameters.AddWithValue("@CreatedBy", transaction.CreatedBy ?? (object)DBNull.Value);

        transaction.TransactionId = (int)(await command.ExecuteScalarAsync() ?? 0);
        return transaction;
    }

    private static Transaction MapTransaction(NpgsqlDataReader reader)
    {
        return new Transaction
        {
            TransactionId = reader.GetInt32(0),
            AccountId = reader.GetInt32(1),
            TransactionType = reader.GetString(2),
            Amount = reader.GetDecimal(3),
            Currency = reader.GetString(4),
            TransactionDate = reader.GetDateTime(5),
            Description = reader.IsDBNull(6) ? null : reader.GetString(6),
            Status = reader.GetString(7),
            ToAccountId = reader.IsDBNull(8) ? null : reader.GetInt32(8),
            ReferenceNumber = reader.IsDBNull(9) ? null : reader.GetString(9),
            BalanceAfter = reader.IsDBNull(10) ? null : reader.GetDecimal(10),
            CreatedBy = reader.IsDBNull(11) ? null : reader.GetString(11)
        };
    }
}