using BankingSystem.API.Models;

namespace BankingSystem.API.Interfaces;

public interface ICustomerRepository
{
    Task<Customer?> GetByIdAsync(int id);
    Task<Customer?> GetByEmailAsync(string email);
    Task<Customer?> GetByPassportNumberAsync(string passportNumber);
    Task<IEnumerable<Customer>> GetAllAsync();
    Task<Customer> CreateAsync(Customer customer);
    Task UpdateAsync(Customer customer);
    Task<bool> EmailExistsAsync(string email);
    Task<bool> PassportExistsAsync(string passportNumber);
}