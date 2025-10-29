using Npgsql;
using BankingSystem.API.Interfaces;
using BankingSystem.API.Models;

namespace BankingSystem.API.Repositories;

public class AccountRepository : IAccountRepository
{
    private readonly string _connectionString;

    public AccountRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task<Account?> GetByIdAsync(int id)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        await using var command = new NpgsqlCommand(@"
            SELECT accountid, customerid, accountnumber, accounttype, 
                   balance, currency, interestrate, status, opendate, closedate, lasttransactiondate
            FROM accounts 
            WHERE accountid = @Id", connection);
        
        command.Parameters.AddWithValue("@Id", id);

        await using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return MapAccount(reader);
        }

        return null;
    }

    public async Task<Account?> GetByAccountNumberAsync(string accountNumber)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        await using var command = new NpgsqlCommand(@"
            SELECT accountid, customerid, accountnumber, accounttype, 
                   balance, currency, interestrate, status, opendate, closedate, lasttransactiondate
            FROM accounts 
            WHERE accountnumber = @AccountNumber", connection);
        
        command.Parameters.AddWithValue("@AccountNumber", accountNumber);

        await using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return MapAccount(reader);
        }

        return null;
    }

    public async Task<IEnumerable<Account>> GetByCustomerIdAsync(int customerId)
    {
        var accounts = new List<Account>();

        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        await using var command = new NpgsqlCommand(@"
            SELECT accountid, customerid, accountnumber, accounttype, 
                   balance, currency, interestrate, status, opendate, closedate, lasttransactiondate
            FROM accounts 
            WHERE customerid = @CustomerId
            ORDER BY opendate DESC", connection);
        
        command.Parameters.AddWithValue("@CustomerId", customerId);

        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            accounts.Add(MapAccount(reader));
        }

        return accounts;
    }

    public async Task<Account> CreateAsync(Account account)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        await using var command = new NpgsqlCommand(@"
            INSERT INTO accounts (customerid, accountnumber, accounttype, balance, currency, interestrate, status, opendate)
            VALUES (@CustomerId, @AccountNumber, @AccountType, @Balance, @Currency, @InterestRate, @Status, @OpenDate)
            RETURNING accountid", connection);

        command.Parameters.AddWithValue("@CustomerId", account.CustomerId);
        command.Parameters.AddWithValue("@AccountNumber", account.AccountNumber);
        command.Parameters.AddWithValue("@AccountType", account.AccountType);
        command.Parameters.AddWithValue("@Balance", account.Balance);
        command.Parameters.AddWithValue("@Currency", account.Currency);
        command.Parameters.AddWithValue("@InterestRate", account.InterestRate);
        command.Parameters.AddWithValue("@Status", account.Status);
        command.Parameters.AddWithValue("@OpenDate", account.OpenDate);

        account.AccountId = (int)(await command.ExecuteScalarAsync() ?? 0);
        return account;
    }

    public async Task UpdateAsync(Account account)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        await using var command = new NpgsqlCommand(@"
            UPDATE accounts 
            SET balance = @Balance,
                status = @Status,
                interestrate = @InterestRate,
                closedate = @CloseDate,
                lasttransactiondate = @LastTransactionDate
            WHERE accountid = @AccountId", connection);

        command.Parameters.AddWithValue("@AccountId", account.AccountId);
        command.Parameters.AddWithValue("@Balance", account.Balance);
        command.Parameters.AddWithValue("@Status", account.Status);
        command.Parameters.AddWithValue("@InterestRate", account.InterestRate);
        command.Parameters.AddWithValue("@CloseDate", account.CloseDate.HasValue ? account.CloseDate.Value : DBNull.Value);
        command.Parameters.AddWithValue("@LastTransactionDate", account.LastTransactionDate.HasValue ? account.LastTransactionDate.Value : DBNull.Value);

        await command.ExecuteNonQueryAsync();
    }

    public async Task<decimal> GetBalanceAsync(int accountId)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        await using var command = new NpgsqlCommand("SELECT balance FROM accounts WHERE accountid = @Id", connection);
        command.Parameters.AddWithValue("@Id", accountId);

        var result = await command.ExecuteScalarAsync();
        return result != null ? (decimal)result : 0;
    }

    private static Account MapAccount(NpgsqlDataReader reader)
    {
        return new Account
        {
            AccountId = reader.GetInt32(0),
            CustomerId = reader.GetInt32(1),
            AccountNumber = reader.GetString(2),
            AccountType = reader.GetString(3),
            Balance = reader.GetDecimal(4),
            Currency = reader.GetString(5),
            InterestRate = reader.GetDecimal(6),
            Status = reader.GetString(7),
            OpenDate = reader.GetDateTime(8),
            CloseDate = reader.IsDBNull(9) ? null : reader.GetDateTime(9),
            LastTransactionDate = reader.IsDBNull(10) ? null : reader.GetDateTime(10)
        };
    }
}