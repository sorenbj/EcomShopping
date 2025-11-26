using EcomShopping.Integration.Abstractions;

namespace EcomShopping.Integration.Core.Providers;

public class MockShippingProvider : IShippingProvider
{
    public string ProviderName => "Mock Shipping";
    public string ProviderType => "Shipping";

    public Task<bool> TestConnectionAsync()
    {
        // Simulate successful connection
        return Task.FromResult(true);
    }

    public Task<decimal> GetShippingRateAsync(object shippingDetails)
    {
        // Simulate rate calculation
        Console.WriteLine($"[Mock Shipping] Calculating shipping rate");
        var rate = 15.99m; // Mock flat rate
        return Task.FromResult(rate);
    }

    public Task<string> BookShipmentAsync(string orderNumber, object shippingDetails)
    {
        // Simulate shipment booking
        var trackingNumber = $"TRACK{DateTime.UtcNow.Ticks}";
        Console.WriteLine($"[Mock Shipping] Booked shipment for order {orderNumber}. Tracking: {trackingNumber}");
        return Task.FromResult(trackingNumber);
    }

    public Task<object> TrackShipmentAsync(string trackingNumber)
    {
        // Simulate tracking lookup
        Console.WriteLine($"[Mock Shipping] Tracking shipment: {trackingNumber}");
        return Task.FromResult<object>(new
        {
            TrackingNumber = trackingNumber,
            Status = "In Transit",
            EstimatedDelivery = DateTime.UtcNow.AddDays(3),
            CurrentLocation = "Distribution Center",
            Events = new[]
            {
                new { Date = DateTime.UtcNow.AddDays(-1), Status = "Picked up", Location = "Origin" },
                new { Date = DateTime.UtcNow, Status = "In transit", Location = "Distribution Center" }
            }
        });
    }
}
