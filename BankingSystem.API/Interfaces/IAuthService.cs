using BankingSystem.API.DTOs;

namespace BankingSystem.API.Interfaces;

public interface IAuthService
{
    Task<AuthResultDto> LoginAsync(string username, string password);
    Task<AuthResultDto> RegisterAsync(RegisterRequest request);
    Task<bool> UserExistsAsync(string username);
    Task<bool> EmailExistsAsync(string email);
}