using Npgsql;
using BankingSystem.API.DTOs;
using BankingSystem.API.Interfaces;
using BankingSystem.API.Models;

namespace BankingSystem.API.Services;

public class TransactionService : ITransactionService
{
    private readonly ITransactionRepository _transactionRepository;
    private readonly string _connectionString;
    private readonly ILogger<TransactionService> _logger;

    public TransactionService(
        ITransactionRepository transactionRepository,
        string connectionString,
        ILogger<TransactionService> logger)
    {
        _transactionRepository = transactionRepository;
        _connectionString = connectionString;
        _logger = logger;
    }

    public async Task<TransactionResultDto> DepositAsync(int accountId, decimal amount, string? description)
    {
        try
        {
            await using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            await using var command = new NpgsqlCommand("SELECT * FROM sp_deposit_funds(@p_account_id, @p_amount, @p_description, @p_created_by)", connection);
            command.Parameters.AddWithValue("@p_account_id", accountId);
            command.Parameters.AddWithValue("@p_amount", amount);
            command.Parameters.AddWithValue("@p_description", description ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@p_created_by", "System");

            await using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                var success = reader.GetBoolean(0);
                var message = reader.GetString(1);
                var transactionId = reader.IsDBNull(2) ? null : (int?)reader.GetInt32(2);
                var newBalance = reader.IsDBNull(3) ? null : (decimal?)reader.GetDecimal(3);

                return new TransactionResultDto
                {
                    Success = success,
                    Message = message,
                    TransactionId = transactionId,
                    NewBalance = newBalance
                };
            }

            return new TransactionResultDto
            {
                Success = false,
                Message = "Ошибка при выполнении депозита"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during deposit");
            return new TransactionResultDto
            {
                Success = false,
                Message = $"Ошибка: {ex.Message}",
                ErrorCode = "DEPOSIT_ERROR"
            };
        }
    }

    public async Task<TransactionResultDto> WithdrawAsync(int accountId, decimal amount, string? description)
    {
        try
        {
            await using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            await using var command = new NpgsqlCommand("SELECT * FROM sp_withdraw_funds(@p_account_id, @p_amount, @p_description, @p_created_by)", connection);
            command.Parameters.AddWithValue("@p_account_id", accountId);
            command.Parameters.AddWithValue("@p_amount", amount);
            command.Parameters.AddWithValue("@p_description", description ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@p_created_by", "System");

            await using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                var success = reader.GetBoolean(0);
                var message = reader.GetString(1);
                var transactionId = reader.IsDBNull(2) ? null : (int?)reader.GetInt32(2);
                var newBalance = reader.IsDBNull(3) ? null : (decimal?)reader.GetDecimal(3);

                return new TransactionResultDto
                {
                    Success = success,
                    Message = message,
                    TransactionId = transactionId,
                    NewBalance = newBalance
                };
            }

            return new TransactionResultDto
            {
                Success = false,
                Message = "Ошибка при снятии средств"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during withdrawal");
            return new TransactionResultDto
            {
                Success = false,
                Message = $"Ошибка: {ex.Message}",
                ErrorCode = "WITHDRAWAL_ERROR"
            };
        }
    }

    public async Task<TransactionResultDto> TransferAsync(int fromAccountId, int toAccountId, decimal amount, string? description)
    {
        try
        {
            await using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            await using var command = new NpgsqlCommand("SELECT * FROM sp_transfer_funds(@p_from_account_id, @p_to_account_id, @p_amount, @p_description, @p_created_by)", connection);
            command.Parameters.AddWithValue("@p_from_account_id", fromAccountId);
            command.Parameters.AddWithValue("@p_to_account_id", toAccountId);
            command.Parameters.AddWithValue("@p_amount", amount);
            command.Parameters.AddWithValue("@p_description", description ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@p_created_by", "System");

            await using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                var success = reader.GetBoolean(0);
                var message = reader.GetString(1);
                var transactionId = reader.IsDBNull(2) ? null : (int?)reader.GetInt32(2);

                return new TransactionResultDto
                {
                    Success = success,
                    Message = message,
                    TransactionId = transactionId
                };
            }

            return new TransactionResultDto
            {
                Success = false,
                Message = "Ошибка при выполнении перевода"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during transfer");
            return new TransactionResultDto
            {
                Success = false,
                Message = $"Ошибка: {ex.Message}",
                ErrorCode = "TRANSFER_ERROR"
            };
        }
    }

    public async Task<IEnumerable<Transaction>> GetAccountTransactionsAsync(int accountId, int page = 1, int pageSize = 20)
    {
        return await _transactionRepository.GetByAccountIdAsync(accountId, page, pageSize);
    }

    public async Task<Transaction?> GetTransactionAsync(int transactionId)
    {
        return await _transactionRepository.GetByIdAsync(transactionId);
    }
}