namespace BankingSystem.API.Models;

public class Account
{
    public int AccountId { get; set; }
    public int CustomerId { get; set; }
    public string AccountNumber { get; set; } = string.Empty;
    public string AccountType { get; set; } = string.Empty; // Checking, Savings, Credit
    public decimal Balance { get; set; }
    public string Currency { get; set; } = "TJS";
    public decimal InterestRate { get; set; }
    public string Status { get; set; } = "Active"; // Active, Frozen, Closed
    public DateTime OpenDate { get; set; }
    public DateTime? CloseDate { get; set; }
    public DateTime? LastTransactionDate { get; set; }
    
    public bool IsActive => Status == "Active";
    public bool CanDebit => IsActive && Balance > 0;
}