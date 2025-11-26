using EcomShopping.Integration.Abstractions;

namespace EcomShopping.Integration.Core.Providers;

public class MockErpIntegration : IErpIntegration
{
    public string ProviderName => "Mock ERP";
    public string ProviderType => "ERP";

    public Task<bool> TestConnectionAsync()
    {
        // Simulate successful connection
        return Task.FromResult(true);
    }

    public Task SyncOrderAsync(string orderNumber)
    {
        // Simulate order sync
        Console.WriteLine($"[Mock ERP] Syncing order: {orderNumber}");
        return Task.CompletedTask;
    }

    public Task UpdateInventoryAsync(string sku, int quantity)
    {
        // Simulate inventory update
        Console.WriteLine($"[Mock ERP] Updating inventory for SKU {sku}: {quantity} units");
        return Task.CompletedTask;
    }

    public Task<object> GetProductDetailsAsync(string sku)
    {
        // Simulate product retrieval
        Console.WriteLine($"[Mock ERP] Getting product details for SKU: {sku}");
        return Task.FromResult<object>(new
        {
            Sku = sku,
            Name = $"Product {sku}",
            Description = "Mock product from ERP",
            Price = 99.99m,
            StockLevel = 100
        });
    }
}
