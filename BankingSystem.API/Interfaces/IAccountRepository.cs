using BankingSystem.API.Models;

namespace BankingSystem.API.Interfaces;

public interface IAccountRepository
{
    Task<Account?> GetByIdAsync(int id);
    Task<Account?> GetByAccountNumberAsync(string accountNumber);
    Task<IEnumerable<Account>> GetByCustomerIdAsync(int customerId);
    Task<Account> CreateAsync(Account account);
    Task UpdateAsync(Account account);
    Task<decimal> GetBalanceAsync(int accountId);
}