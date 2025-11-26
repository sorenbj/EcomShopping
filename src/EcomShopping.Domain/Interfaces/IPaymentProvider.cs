namespace EcomShopping.Domain.Interfaces;

public interface IPaymentProvider
{
    Task<PaymentResult> AuthorizePaymentAsync(PaymentRequest request);
    Task<PaymentResult> CapturePaymentAsync(string transactionId, decimal amount);
    Task<PaymentResult> RefundPaymentAsync(string transactionId, decimal amount);
    Task<PaymentStatusResult> GetPaymentStatusAsync(string transactionId);
}

public class PaymentRequest
{
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "USD";
    public string? CardNumber { get; set; }
    public string? CardHolderName { get; set; }
    public string? ExpiryMonth { get; set; }
    public string? ExpiryYear { get; set; }
    public string? Cvv { get; set; }
    public Dictionary<string, string> Metadata { get; set; } = new();
}

public class PaymentResult
{
    public bool Success { get; set; }
    public string? TransactionId { get; set; }
    public string? ErrorMessage { get; set; }
    public string? ErrorCode { get; set; }
    public Dictionary<string, string> Metadata { get; set; } = new();
}

public class PaymentStatusResult
{
    public bool Success { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? TransactionId { get; set; }
    public decimal? Amount { get; set; }
}
