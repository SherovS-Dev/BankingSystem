using BankingSystem.API.Models;

namespace BankingSystem.API.Interfaces;

public interface ITransactionRepository
{
    Task<Transaction?> GetByIdAsync(int id);
    Task<Transaction?> GetByReferenceNumberAsync(string referenceNumber);
    Task<IEnumerable<Transaction>> GetByAccountIdAsync(int accountId, int page = 1, int pageSize = 20);
    Task<IEnumerable<Transaction>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<Transaction> CreateAsync(Transaction transaction);
}