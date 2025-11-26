# Integration Engine - Developer Guide

## Overview

The Integration Engine provides a comprehensive, extensible framework for integrating with external systems including ERP, CRM, Shipping providers, and Payment gateways. It supports manual execution, event-based triggers, and scheduled integrations.

## Architecture

### Components

1. **Integration.Abstractions**: Interface definitions for all integration providers
2. **Integration.Core**: Core implementation including engine, registry, scheduler, and mock providers
3. **Domain Entities**: Integration configuration, execution tracking, and scheduling
4. **API Controller**: RESTful endpoints for managing integrations

### Key Classes

- `IntegrationProviderRegistry`: Central registry for managing integration providers
- `IntegrationEngine`: Executes integration operations and manages provider lifecycle
- `IntegrationScheduler`: Handles scheduled and recurring integration tasks
- `IntegrationConfiguration`: Database entity storing provider configurations
- `IntegrationExecution`: Tracks execution history and results
- `IntegrationSchedule`: Defines scheduling rules for automated integrations

## Integration Types

### 1. ERP Integration (IErpIntegration)

Connect to ERP systems for order sync, inventory management, and product updates.

**Operations:**
- `SyncOrderAsync(orderNumber)`: Synchronize order to ERP
- `UpdateInventoryAsync(sku, quantity)`: Update inventory levels
- `GetProductDetailsAsync(sku)`: Retrieve product information

**Example:**
```csharp
public class SapErpIntegration : IErpIntegration
{
    public string ProviderName => "SAP ERP";
    public string ProviderType => "ERP";
    
    public async Task<bool> TestConnectionAsync()
    {
        // Test SAP connection
        return await CheckSapConnectionAsync();
    }
    
    public async Task SyncOrderAsync(string orderNumber)
    {
        // Send order to SAP
        await _sapClient.CreateOrderAsync(orderNumber);
    }
    
    public async Task UpdateInventoryAsync(string sku, int quantity)
    {
        // Update SAP inventory
        await _sapClient.UpdateStockAsync(sku, quantity);
    }
    
    public async Task<object> GetProductDetailsAsync(string sku)
    {
        // Retrieve from SAP
        return await _sapClient.GetProductAsync(sku);
    }
}
```

### 2. CRM Integration (ICrmIntegration)

Sync customer data with CRM systems.

**Operations:**
- `SyncCustomerAsync(userId, customerData)`: Sync customer information
- `GetCustomerDataAsync(userId)`: Retrieve customer details

**Example:**
```csharp
public class SalesforceCrmIntegration : ICrmIntegration
{
    public string ProviderName => "Salesforce";
    public string ProviderType => "CRM";
    
    public async Task SyncCustomerAsync(string userId, object customerData)
    {
        // Update Salesforce contact
        await _salesforceClient.UpsertContactAsync(userId, customerData);
    }
    
    public async Task<object> GetCustomerDataAsync(string userId)
    {
        return await _salesforceClient.GetContactAsync(userId);
    }
}
```

### 3. Shipping Provider (IShippingProvider)

Calculate rates, book shipments, and track packages.

**Operations:**
- `GetShippingRateAsync(shippingDetails)`: Calculate shipping cost
- `BookShipmentAsync(orderNumber, shippingDetails)`: Create shipment and get tracking
- `TrackShipmentAsync(trackingNumber)`: Get tracking information

**Example:**
```csharp
public class FedExShippingProvider : IShippingProvider
{
    public string ProviderName => "FedEx";
    public string ProviderType => "Shipping";
    
    public async Task<decimal> GetShippingRateAsync(object shippingDetails)
    {
        var rate = await _fedexClient.GetRateAsync(shippingDetails);
        return rate.TotalCharge;
    }
    
    public async Task<string> BookShipmentAsync(string orderNumber, object shippingDetails)
    {
        var shipment = await _fedexClient.CreateShipmentAsync(shippingDetails);
        return shipment.TrackingNumber;
    }
    
    public async Task<object> TrackShipmentAsync(string trackingNumber)
    {
        return await _fedexClient.TrackAsync(trackingNumber);
    }
}
```

### 4. Payment Provider (IPaymentProvider)

Process payments, refunds, and check transaction status.

**Operations:**
- `ProcessPaymentAsync(amount, paymentDetails)`: Process a payment
- `RefundPaymentAsync(transactionId, amount)`: Issue a refund
- `GetPaymentStatusAsync(transactionId)`: Check payment status

**Example:**
```csharp
public class StripePaymentProvider : IPaymentProvider
{
    public string ProviderName => "Stripe";
    public string ProviderType => "Payment";
    
    public async Task<object> ProcessPaymentAsync(decimal amount, object paymentDetails)
    {
        var charge = await _stripeClient.CreateChargeAsync(amount, paymentDetails);
        return new { TransactionId = charge.Id, Status = charge.Status };
    }
    
    public async Task<object> RefundPaymentAsync(string transactionId, decimal amount)
    {
        var refund = await _stripeClient.RefundAsync(transactionId, amount);
        return new { RefundId = refund.Id, Status = refund.Status };
    }
    
    public async Task<object> GetPaymentStatusAsync(string transactionId)
    {
        var charge = await _stripeClient.GetChargeAsync(transactionId);
        return new { Status = charge.Status, Amount = charge.Amount };
    }
}
```

## Registering Providers

### In Program.cs

```csharp
// Register integration services
builder.Services.AddSingleton<IntegrationProviderRegistry>();
builder.Services.AddSingleton<IntegrationEngine>();
builder.Services.AddSingleton<IntegrationScheduler>();

// Register providers
builder.Services.AddSingleton(sp =>
{
    var registry = sp.GetRequiredService<IntegrationProviderRegistry>();
    
    // Register your custom providers
    registry.Register("sap-erp", new SapErpIntegration(/* config */));
    registry.Register("salesforce-crm", new SalesforceCrmIntegration(/* config */));
    registry.Register("fedex-shipping", new FedExShippingProvider(/* config */));
    registry.Register("stripe-payment", new StripePaymentProvider(/* config */));
    
    return registry;
});
```

## API Usage

### 1. List All Providers

```http
GET /api/integrations/providers
```

**Response:**
```json
[
  {
    "providerName": "Mock ERP",
    "providerType": "ERP"
  },
  {
    "providerName": "Mock CRM",
    "providerType": "CRM"
  }
]
```

### 2. Test Provider Connection

```http
GET /api/integrations/providers/{providerKey}/test
```

**Response:**
```json
{
  "success": true,
  "providerName": "Mock ERP"
}
```

### 3. Execute Integration

```http
POST /api/integrations/execute
Content-Type: application/json

{
  "providerKey": "mock-erp",
  "operation": "getproduct",
  "parameters": "SKU123"
}
```

**Response:**
```json
{
  "success": true,
  "message": "Product retrieved successfully",
  "data": {
    "sku": "SKU123",
    "name": "Product SKU123",
    "price": 99.99,
    "stockLevel": 100
  }
}
```

### 4. Add Schedule

```http
POST /api/integrations/schedules
Content-Type: application/json

{
  "scheduleId": 1,
  "providerKey": "mock-erp",
  "operation": "syncorder",
  "intervalMinutes": 30,
  "parameters": "ORD123"
}
```

### 5. List Schedules

```http
GET /api/integrations/schedules
```

**Response:**
```json
[
  {
    "scheduleId": 1,
    "providerKey": "mock-erp",
    "operation": "syncorder",
    "intervalMinutes": 30,
    "lastExecutionTime": "2024-01-15T10:30:00Z",
    "nextExecutionTime": "2024-01-15T11:00:00Z"
  }
]
```

### 6. Execute Schedules Manually

```http
POST /api/integrations/schedules/execute
```

**Response:**
```json
{
  "success": true,
  "executedCount": 2,
  "results": [
    {
      "scheduleId": 1,
      "success": true,
      "message": "Order synced successfully",
      "executedAt": "2024-01-15T10:45:00Z"
    }
  ]
}
```

### 7. Remove Schedule

```http
DELETE /api/integrations/schedules/{scheduleId}
```

## Configuration

### appsettings.json

```json
{
  "IntegrationSettings": {
    "Providers": {
      "sap-erp": {
        "Name": "SAP ERP System",
        "Type": "ERP",
        "ProviderClass": "SapErpIntegration",
        "Enabled": true,
        "Settings": {
          "ApiEndpoint": "https://sap.company.com/api",
          "Timeout": "60",
          "Username": "integration_user"
        }
      },
      "salesforce-crm": {
        "Name": "Salesforce CRM",
        "Type": "CRM",
        "ProviderClass": "SalesforceCrmIntegration",
        "Enabled": true,
        "Settings": {
          "ApiEndpoint": "https://company.salesforce.com",
          "ApiVersion": "v52.0"
        }
      }
    }
  }
}
```

## Secure Credential Management

### Best Practices

1. **Never store credentials in appsettings.json**
2. **Use Azure Key Vault or similar secret management**
3. **Use environment variables for sensitive data**
4. **Implement credential encryption at rest**

### Example with Azure Key Vault

```csharp
public class SecureProviderFactory
{
    private readonly IConfiguration _configuration;
    private readonly SecretClient _keyVaultClient;

    public async Task<IErpIntegration> CreateSapIntegrationAsync()
    {
        var apiKey = await _keyVaultClient.GetSecretAsync("SAP-API-Key");
        var config = new SapConfiguration
        {
            ApiEndpoint = _configuration["IntegrationSettings:Providers:sap-erp:Settings:ApiEndpoint"],
            ApiKey = apiKey.Value.Value
        };
        return new SapErpIntegration(config);
    }
}
```

## Event-Based Triggers

### Order Created Event

```csharp
public class OrderCreatedEventHandler
{
    private readonly IntegrationEngine _integrationEngine;

    public async Task HandleAsync(OrderCreatedEvent evt)
    {
        // Sync to ERP
        await _integrationEngine.ExecuteAsync("sap-erp", "syncorder", evt.OrderNumber);
        
        // Sync customer to CRM
        await _integrationEngine.ExecuteAsync("salesforce-crm", "synccustomer", new
        {
            userId = evt.CustomerId,
            customerData = evt.CustomerData
        });
    }
}
```

### Inventory Changed Event

```csharp
public class InventoryChangedEventHandler
{
    private readonly IntegrationEngine _integrationEngine;

    public async Task HandleAsync(InventoryChangedEvent evt)
    {
        await _integrationEngine.ExecuteAsync("sap-erp", "updateinventory", new
        {
            sku = evt.Sku,
            quantity = evt.NewQuantity
        });
    }
}
```

## Scheduled Integrations

### Background Service for Scheduler

```csharp
public class IntegrationSchedulerService : BackgroundService
{
    private readonly IntegrationScheduler _scheduler;
    private readonly ILogger<IntegrationSchedulerService> _logger;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await _scheduler.ExecuteDueSchedulesAsync();
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in scheduler service");
            }
        }
    }
}
```

Register in Program.cs:
```csharp
builder.Services.AddHostedService<IntegrationSchedulerService>();
```

## Error Handling

### Retry Logic

```csharp
public class ResilientIntegrationEngine : IntegrationEngine
{
    public async Task<IntegrationResult> ExecuteWithRetryAsync(
        string providerKey, 
        string operation, 
        object? parameters = null,
        int maxRetries = 3)
    {
        for (int attempt = 0; attempt < maxRetries; attempt++)
        {
            try
            {
                var result = await ExecuteAsync(providerKey, operation, parameters);
                if (result.IsSuccess)
                    return result;
                    
                if (attempt < maxRetries - 1)
                    await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, attempt)));
            }
            catch (Exception ex) when (attempt < maxRetries - 1)
            {
                _logger.LogWarning(ex, "Integration attempt {Attempt} failed", attempt + 1);
                await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, attempt)));
            }
        }
        
        return IntegrationResult.Failure("Max retries exceeded");
    }
}
```

## Testing

### Unit Tests

```csharp
[Fact]
public async Task ExecuteAsync_WithValidProvider_ShouldSucceed()
{
    // Arrange
    var registry = new IntegrationProviderRegistry();
    registry.Register("test-erp", new MockErpIntegration());
    var engine = new IntegrationEngine(registry, NullLogger<IntegrationEngine>.Instance);

    // Act
    var result = await engine.ExecuteAsync("test-erp", "getproduct", "SKU123");

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Data.Should().NotBeNull();
}
```

### Mock Providers

The framework includes mock implementations for all provider types:
- `MockErpIntegration`
- `MockCrmIntegration`
- `MockShippingProvider`
- `MockPaymentProvider`

Use these for development and testing before implementing real providers.

## Performance Considerations

1. **Connection Pooling**: Reuse connections to external systems
2. **Caching**: Cache frequently accessed data
3. **Async/Await**: All operations are asynchronous
4. **Batching**: Group multiple operations when possible
5. **Circuit Breaker**: Implement circuit breaker pattern for failing providers

## Monitoring and Logging

### Logging Example

```csharp
_logger.LogInformation(
    "Integration executed: Provider={Provider}, Operation={Operation}, Duration={Duration}ms",
    providerKey,
    operation,
    stopwatch.ElapsedMilliseconds);
```

### Execution Tracking

Use `IntegrationExecution` entity to track:
- Execution time
- Success/failure status
- Error messages
- Results
- Triggered by (user/schedule/event)

## Migration Guide

### Adding to Existing System

1. Add NuGet packages
2. Register services in DI container
3. Create provider implementations
4. Configure in appsettings.json
5. Add API controller endpoints
6. Implement event handlers
7. Add background scheduler service

## Troubleshooting

### Common Issues

**Provider Not Found**
- Check provider key matches registration
- Verify provider is registered in DI container

**Connection Test Fails**
- Verify credentials are correct
- Check network connectivity
- Review firewall rules
- Validate API endpoint URLs

**Schedule Not Executing**
- Ensure background service is running
- Check schedule interval is appropriate
- Verify provider is enabled

## Next Steps

1. Implement custom providers for your specific systems
2. Add database persistence for configurations and executions
3. Implement webhook support for real-time events
4. Add UI for managing integrations
5. Implement advanced scheduling (cron expressions)
6. Add monitoring dashboard
7. Implement rate limiting and throttling
