using System.ComponentModel.DataAnnotations;

namespace BankingSystem.API.DTOs;

public class WithdrawalRequest
{
    [Required]
    public int AccountId { get; set; }
    
    [Required]
    [Range(0.01, 50000, ErrorMessage = "Сумма должна быть от 0.01 до 50,000")]
    public decimal Amount { get; set; }
    
    [MaxLength(200)]
    public string? Description { get; set; }
}