namespace EcomShopping.Integration.Abstractions;

public interface IPaymentProvider : IIntegrationProvider
{
    Task<object> ProcessPaymentAsync(decimal amount, object paymentDetails);
    Task<object> RefundPaymentAsync(string transactionId, decimal amount);
    Task<object> GetPaymentStatusAsync(string transactionId);
}
