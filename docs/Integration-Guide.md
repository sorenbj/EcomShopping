# Integration Guide

This guide explains how to implement and use the integration engine for connecting with external systems.

## Overview

The integration engine provides a modular framework for integrating with various external systems including:
- **ERP Systems** (order sync, inventory updates, product data)
- **CRM Systems** (customer data synchronization)
- **Shipping Providers** (rate calculation, booking, tracking)
- **Payment Gateways** (payment processing, refunds)

## Architecture

The integration system is built on two layers:

### 1. Integration.Abstractions
Contains interface definitions for all integration types.

### 2. Integration.Core
Contains concrete implementations, provider registry, and orchestration logic.

## Integration Interfaces

### IIntegrationProvider (Base Interface)

All integration providers must implement this base interface:

```csharp
public interface IIntegrationProvider
{
    string ProviderName { get; }
    string ProviderType { get; }
    Task<bool> TestConnectionAsync();
}
```

### IErpIntegration

For ERP system integrations:

```csharp
public interface IErpIntegration : IIntegrationProvider
{
    Task SyncOrderAsync(string orderNumber);
    Task UpdateInventoryAsync(string sku, int quantity);
    Task<object> GetProductDetailsAsync(string sku);
}
```

**Example Implementation:**

```csharp
public class SapErpIntegration : IErpIntegration
{
    private readonly SapConfiguration _config;
    
    public string ProviderName => "SAP ERP";
    public string ProviderType => "ERP";
    
    public async Task<bool> TestConnectionAsync()
    {
        // Test SAP connection
        return await CheckSapConnection();
    }
    
    public async Task SyncOrderAsync(string orderNumber)
    {
        // Sync order to SAP
        await SendOrderToSap(orderNumber);
    }
    
    public async Task UpdateInventoryAsync(string sku, int quantity)
    {
        // Update inventory in SAP
        await UpdateSapInventory(sku, quantity);
    }
    
    public async Task<object> GetProductDetailsAsync(string sku)
    {
        // Get product details from SAP
        return await FetchFromSap(sku);
    }
}
```

### ICrmIntegration

For CRM system integrations:

```csharp
public interface ICrmIntegration : IIntegrationProvider
{
    Task SyncCustomerAsync(string userId, object customerData);
    Task<object> GetCustomerDataAsync(string userId);
}
```

**Example Implementation:**

```csharp
public class SalesforceCrmIntegration : ICrmIntegration
{
    public string ProviderName => "Salesforce";
    public string ProviderType => "CRM";
    
    public async Task<bool> TestConnectionAsync()
    {
        return await TestSalesforceConnection();
    }
    
    public async Task SyncCustomerAsync(string userId, object customerData)
    {
        await UpdateSalesforceContact(userId, customerData);
    }
    
    public async Task<object> GetCustomerDataAsync(string userId)
    {
        return await GetSalesforceContact(userId);
    }
}
```

### IShippingProvider

For shipping carrier integrations:

```csharp
public interface IShippingProvider : IIntegrationProvider
{
    Task<decimal> GetShippingRateAsync(object shippingDetails);
    Task<string> BookShipmentAsync(string orderNumber, object shippingDetails);
    Task<object> TrackShipmentAsync(string trackingNumber);
}
```

**Example Implementation:**

```csharp
public class FedExShippingProvider : IShippingProvider
{
    public string ProviderName => "FedEx";
    public string ProviderType => "Shipping";
    
    public async Task<bool> TestConnectionAsync()
    {
        return await TestFedExApi();
    }
    
    public async Task<decimal> GetShippingRateAsync(object shippingDetails)
    {
        // Calculate shipping rate
        var rate = await CalculateFedExRate(shippingDetails);
        return rate;
    }
    
    public async Task<string> BookShipmentAsync(string orderNumber, object shippingDetails)
    {
        // Book shipment and return tracking number
        var trackingNumber = await CreateFedExShipment(orderNumber, shippingDetails);
        return trackingNumber;
    }
    
    public async Task<object> TrackShipmentAsync(string trackingNumber)
    {
        return await GetFedExTracking(trackingNumber);
    }
}
```

### IPaymentProvider

For payment gateway integrations:

```csharp
public interface IPaymentProvider : IIntegrationProvider
{
    Task<object> ProcessPaymentAsync(decimal amount, object paymentDetails);
    Task<object> RefundPaymentAsync(string transactionId, decimal amount);
    Task<object> GetPaymentStatusAsync(string transactionId);
}
```

**Example Implementation:**

```csharp
public class StripePaymentProvider : IPaymentProvider
{
    public string ProviderName => "Stripe";
    public string ProviderType => "Payment";
    
    public async Task<bool> TestConnectionAsync()
    {
        return await TestStripeApi();
    }
    
    public async Task<object> ProcessPaymentAsync(decimal amount, object paymentDetails)
    {
        // Process payment through Stripe
        var charge = await CreateStripeCharge(amount, paymentDetails);
        return new { TransactionId = charge.Id, Status = charge.Status };
    }
    
    public async Task<object> RefundPaymentAsync(string transactionId, decimal amount)
    {
        var refund = await CreateStripeRefund(transactionId, amount);
        return new { RefundId = refund.Id, Status = refund.Status };
    }
    
    public async Task<object> GetPaymentStatusAsync(string transactionId)
    {
        var charge = await RetrieveStripeCharge(transactionId);
        return new { Status = charge.Status, Amount = charge.Amount };
    }
}
```

## Provider Registry

Create a provider registry to manage multiple implementations:

```csharp
public class IntegrationProviderRegistry
{
    private readonly Dictionary<string, IIntegrationProvider> _providers = new();
    
    public void Register<T>(string key, T provider) where T : IIntegrationProvider
    {
        _providers[key] = provider;
    }
    
    public T GetProvider<T>(string key) where T : IIntegrationProvider
    {
        if (_providers.TryGetValue(key, out var provider) && provider is T typedProvider)
        {
            return typedProvider;
        }
        throw new InvalidOperationException($"Provider '{key}' not found or wrong type");
    }
    
    public IEnumerable<T> GetAllProviders<T>() where T : IIntegrationProvider
    {
        return _providers.Values.OfType<T>();
    }
}
```

## Configuration

### appsettings.json

```json
{
  "Integrations": {
    "ERP": {
      "Provider": "SAP",
      "ApiEndpoint": "https://sap.example.com/api",
      "ApiKey": "your-api-key",
      "Enabled": true
    },
    "CRM": {
      "Provider": "Salesforce",
      "ApiEndpoint": "https://salesforce.example.com/api",
      "ClientId": "your-client-id",
      "ClientSecret": "your-client-secret",
      "Enabled": true
    },
    "Shipping": {
      "Providers": [
        {
          "Name": "FedEx",
          "AccountNumber": "123456",
          "ApiKey": "your-api-key",
          "Enabled": true
        },
        {
          "Name": "UPS",
          "AccountNumber": "789012",
          "ApiKey": "your-api-key",
          "Enabled": false
        }
      ]
    },
    "Payment": {
      "Provider": "Stripe",
      "PublicKey": "pk_test_xxx",
      "SecretKey": "sk_test_xxx",
      "WebhookSecret": "whsec_xxx",
      "Enabled": true
    }
  }
}
```

### Configuration Models

```csharp
public class IntegrationSettings
{
    public ErpSettings ERP { get; set; }
    public CrmSettings CRM { get; set; }
    public ShippingSettings Shipping { get; set; }
    public PaymentSettings Payment { get; set; }
}

public class ErpSettings
{
    public string Provider { get; set; }
    public string ApiEndpoint { get; set; }
    public string ApiKey { get; set; }
    public bool Enabled { get; set; }
}

// Similar settings classes for CRM, Shipping, Payment
```

## Dependency Injection Setup

In `Program.cs`:

```csharp
// Configure integration settings
builder.Services.Configure<IntegrationSettings>(
    builder.Configuration.GetSection("Integrations"));

// Register provider registry
builder.Services.AddSingleton<IntegrationProviderRegistry>();

// Register specific providers
builder.Services.AddScoped<IErpIntegration, SapErpIntegration>();
builder.Services.AddScoped<ICrmIntegration, SalesforceCrmIntegration>();
builder.Services.AddScoped<IShippingProvider, FedExShippingProvider>();
builder.Services.AddScoped<IPaymentProvider, StripePaymentProvider>();
```

## Usage Examples

### Using ERP Integration

```csharp
public class OrderService
{
    private readonly IErpIntegration _erpIntegration;
    
    public OrderService(IErpIntegration erpIntegration)
    {
        _erpIntegration = erpIntegration;
    }
    
    public async Task ProcessOrder(Order order)
    {
        // Process order locally
        await SaveOrder(order);
        
        // Sync to ERP
        await _erpIntegration.SyncOrderAsync(order.OrderNumber);
        
        // Update inventory
        foreach (var item in order.Items)
        {
            await _erpIntegration.UpdateInventoryAsync(
                item.Product.SKU, 
                -item.Quantity);
        }
    }
}
```

### Using Shipping Provider

```csharp
public class ShippingService
{
    private readonly IShippingProvider _shippingProvider;
    
    public async Task<decimal> CalculateShippingCost(Address destination, decimal weight)
    {
        var shippingDetails = new
        {
            Destination = destination,
            Weight = weight,
            ServiceType = "Express"
        };
        
        return await _shippingProvider.GetShippingRateAsync(shippingDetails);
    }
    
    public async Task<string> BookShipment(Order order)
    {
        var shippingDetails = new
        {
            Destination = order.ShippingAddress,
            Items = order.Items,
            ServiceType = "Express"
        };
        
        var trackingNumber = await _shippingProvider.BookShipmentAsync(
            order.OrderNumber, 
            shippingDetails);
            
        return trackingNumber;
    }
}
```

### Using Payment Provider

```csharp
public class PaymentService
{
    private readonly IPaymentProvider _paymentProvider;
    
    public async Task<bool> ProcessPayment(Order order, PaymentInfo paymentInfo)
    {
        var paymentDetails = new
        {
            CardNumber = paymentInfo.CardNumber,
            ExpiryMonth = paymentInfo.ExpiryMonth,
            ExpiryYear = paymentInfo.ExpiryYear,
            CVV = paymentInfo.CVV,
            Currency = "USD"
        };
        
        var result = await _paymentProvider.ProcessPaymentAsync(
            order.TotalAmount, 
            paymentDetails);
            
        // Store transaction ID
        order.PaymentTransactionId = result.TransactionId;
        
        return result.Status == "succeeded";
    }
}
```

## Error Handling

Implement robust error handling for integration failures:

```csharp
public class ResilientIntegrationService
{
    private readonly IErpIntegration _erpIntegration;
    private readonly ILogger<ResilientIntegrationService> _logger;
    
    public async Task<bool> SyncOrderWithRetry(string orderNumber, int maxRetries = 3)
    {
        for (int attempt = 1; attempt <= maxRetries; attempt++)
        {
            try
            {
                await _erpIntegration.SyncOrderAsync(orderNumber);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, 
                    "Failed to sync order {OrderNumber} on attempt {Attempt}", 
                    orderNumber, attempt);
                
                if (attempt == maxRetries)
                {
                    _logger.LogError(ex, 
                        "All retry attempts failed for order {OrderNumber}", 
                        orderNumber);
                    return false;
                }
                
                // Exponential backoff
                await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, attempt)));
            }
        }
        
        return false;
    }
}
```

## Testing

### Mock Implementations

For testing, create mock implementations:

```csharp
public class MockErpIntegration : IErpIntegration
{
    public string ProviderName => "Mock ERP";
    public string ProviderType => "ERP";
    
    public Task<bool> TestConnectionAsync() => Task.FromResult(true);
    
    public Task SyncOrderAsync(string orderNumber)
    {
        // Simulate success
        return Task.CompletedTask;
    }
    
    public Task UpdateInventoryAsync(string sku, int quantity)
    {
        return Task.CompletedTask;
    }
    
    public Task<object> GetProductDetailsAsync(string sku)
    {
        return Task.FromResult<object>(new { SKU = sku, Name = "Mock Product" });
    }
}
```

## Best Practices

1. **Async Operations**: All integration methods should be async
2. **Error Handling**: Always handle and log integration failures
3. **Retry Logic**: Implement retry with exponential backoff
4. **Circuit Breaker**: Use circuit breaker pattern for failing services
5. **Logging**: Log all integration attempts and results
6. **Configuration**: Use configuration for endpoints and credentials
7. **Testing**: Test with mock implementations first
8. **Monitoring**: Monitor integration health and performance
9. **Versioning**: Handle API version changes gracefully
10. **Security**: Securely store API keys and credentials

## Troubleshooting

### Connection Issues
- Verify API credentials
- Check network connectivity
- Review firewall rules
- Test with provider's sandbox environment

### Data Sync Issues
- Validate data format
- Check for required fields
- Review provider's API documentation
- Enable verbose logging

### Performance Issues
- Implement caching where appropriate
- Use batch operations when available
- Monitor API rate limits
- Consider async processing for large operations
