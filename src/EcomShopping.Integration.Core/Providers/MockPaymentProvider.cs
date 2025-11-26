using EcomShopping.Integration.Abstractions;

namespace EcomShopping.Integration.Core.Providers;

public class MockPaymentProvider : IPaymentProvider
{
    public string ProviderName => "Mock Payment Gateway";
    public string ProviderType => "Payment";

    public Task<bool> TestConnectionAsync()
    {
        // Simulate successful connection
        return Task.FromResult(true);
    }

    public Task<object> ProcessPaymentAsync(decimal amount, object paymentDetails)
    {
        // Simulate payment processing
        var transactionId = $"TXN{DateTime.UtcNow.Ticks}";
        Console.WriteLine($"[Mock Payment] Processing payment of ${amount}. Transaction: {transactionId}");
        return Task.FromResult<object>(new
        {
            TransactionId = transactionId,
            Status = "Approved",
            Amount = amount,
            ProcessedAt = DateTime.UtcNow,
            AuthorizationCode = $"AUTH{new Random().Next(100000, 999999)}"
        });
    }

    public Task<object> RefundPaymentAsync(string transactionId, decimal amount)
    {
        // Simulate refund processing
        var refundId = $"REF{DateTime.UtcNow.Ticks}";
        Console.WriteLine($"[Mock Payment] Processing refund of ${amount} for transaction {transactionId}. Refund: {refundId}");
        return Task.FromResult<object>(new
        {
            RefundId = refundId,
            TransactionId = transactionId,
            Status = "Refunded",
            Amount = amount,
            ProcessedAt = DateTime.UtcNow
        });
    }

    public Task<object> GetPaymentStatusAsync(string transactionId)
    {
        // Simulate status lookup
        Console.WriteLine($"[Mock Payment] Getting status for transaction: {transactionId}");
        return Task.FromResult<object>(new
        {
            TransactionId = transactionId,
            Status = "Completed",
            Amount = 99.99m,
            ProcessedAt = DateTime.UtcNow.AddHours(-1)
        });
    }
}
