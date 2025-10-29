using BankingSystem.API.DTOs;
using BankingSystem.API.Models;

namespace BankingSystem.API.Interfaces;

public interface ITransactionService
{
    Task<TransactionResultDto> DepositAsync(int accountId, decimal amount, string? description);
    Task<TransactionResultDto> WithdrawAsync(int accountId, decimal amount, string? description);
    Task<TransactionResultDto> TransferAsync(int fromAccountId, int toAccountId, decimal amount, string? description);
    Task<IEnumerable<Transaction>> GetAccountTransactionsAsync(int accountId, int page = 1, int pageSize = 20);
    Task<Transaction?> GetTransactionAsync(int transactionId);
}