using System.ComponentModel.DataAnnotations;

namespace BankingSystem.API.DTOs;

public class DepositRequest
{
    [Required]
    public int AccountId { get; set; }
    
    [Required]
    [Range(0.01, 10000000, ErrorMessage = "Сумма должна быть от 0.01 до 10,000,000")]
    public decimal Amount { get; set; }
    
    [MaxLength(200)]
    public string? Description { get; set; }
}