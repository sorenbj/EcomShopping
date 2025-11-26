namespace EcomShopping.Integration.Abstractions;

public interface IErpIntegration : IIntegrationProvider
{
    Task SyncOrderAsync(string orderNumber);
    Task UpdateInventoryAsync(string sku, int quantity);
    Task<object> GetProductDetailsAsync(string sku);
}
