namespace BankingSystem.API.DTOs;

public class TransactionResultDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public int? TransactionId { get; set; }
    public string? ReferenceNumber { get; set; }
    public decimal? NewBalance { get; set; }
    public string? ErrorCode { get; set; }
}