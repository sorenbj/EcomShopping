namespace EcomShopping.Integration.Abstractions;

public interface IShippingProvider : IIntegrationProvider
{
    Task<decimal> GetShippingRateAsync(object shippingDetails);
    Task<string> BookShipmentAsync(string orderNumber, object shippingDetails);
    Task<object> TrackShipmentAsync(string trackingNumber);
}
