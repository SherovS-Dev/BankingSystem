using System.ComponentModel.DataAnnotations;

namespace BankingSystem.API.DTOs;

public class TransferRequest
{
    [Required]
    public int FromAccountId { get; set; }
    
    [Required]
    public int ToAccountId { get; set; }
    
    [Required]
    [Range(0.01, 1000000, ErrorMessage = "Сумма должна быть от 0.01 до 1,000,000")]
    public decimal Amount { get; set; }
    
    [MaxLength(200)]
    public string? Description { get; set; }
}