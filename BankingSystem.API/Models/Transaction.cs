namespace BankingSystem.API.Models;

public class Transaction
{
    public int TransactionId { get; set; }
    public int AccountId { get; set; }
    public string TransactionType { get; set; } = string.Empty; // Deposit, Withdrawal, Transfer
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "TJS";
    public DateTime TransactionDate { get; set; }
    public string? Description { get; set; }
    public string Status { get; set; } = "Completed"; // Pending, Completed, Failed, Cancelled
    public int? ToAccountId { get; set; }
    public string? ReferenceNumber { get; set; }
    public decimal? BalanceAfter { get; set; }
    public string? CreatedBy { get; set; }
    
    public bool IsCompleted => Status == "Completed";
    public bool IsTransfer => TransactionType == "Transfer";
}