using EcomShopping.Integration.Abstractions;

namespace EcomShopping.Integration.Core.Providers;

public class MockCrmIntegration : ICrmIntegration
{
    public string ProviderName => "Mock CRM";
    public string ProviderType => "CRM";

    public Task<bool> TestConnectionAsync()
    {
        // Simulate successful connection
        return Task.FromResult(true);
    }

    public Task SyncCustomerAsync(string userId, object customerData)
    {
        // Simulate customer sync
        Console.WriteLine($"[Mock CRM] Syncing customer: {userId}");
        return Task.CompletedTask;
    }

    public Task<object> GetCustomerDataAsync(string userId)
    {
        // Simulate customer retrieval
        Console.WriteLine($"[Mock CRM] Getting customer data for: {userId}");
        return Task.FromResult<object>(new
        {
            UserId = userId,
            Name = "John Doe",
            Email = "john.doe@example.com",
            Phone = "+1-555-0100",
            CustomerSince = DateTime.UtcNow.AddYears(-2),
            TotalOrders = 15,
            LifetimeValue = 1250.00m
        });
    }
}
