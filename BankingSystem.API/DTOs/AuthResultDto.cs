using BankingSystem.API.Models;

namespace BankingSystem.API.DTOs;

public class AuthResultDto
{
    public bool Success { get; set; }
    public string? Token { get; set; }
    public string Message { get; set; } = string.Empty;
    public User? User { get; set; }
}