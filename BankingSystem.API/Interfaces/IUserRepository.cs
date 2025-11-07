using BankingSystem.API.Models;

namespace BankingSystem.API.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(int id);
    Task<User?> GetByUsernameAsync(string username);
    Task<User?> GetByEmailAsync(string email);
    Task<User> CreateAsync(User user);
    Task UpdateAsync(User user);
    Task UpdateLastLoginAsync(int userId);
    Task IncrementFailedLoginAttemptsAsync(int userId);
    Task ResetFailedLoginAttemptsAsync(int userId);
    Task LockUserAsync(int userId, DateTime lockUntil);
}