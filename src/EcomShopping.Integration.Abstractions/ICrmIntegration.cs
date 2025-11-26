namespace EcomShopping.Integration.Abstractions;

public interface ICrmIntegration : IIntegrationProvider
{
    Task SyncCustomerAsync(string userId, object customerData);
    Task<object> GetCustomerDataAsync(string userId);
}
