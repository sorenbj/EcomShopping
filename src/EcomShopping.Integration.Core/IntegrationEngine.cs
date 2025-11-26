using EcomShopping.Integration.Abstractions;
using Microsoft.Extensions.Logging;

namespace EcomShopping.Integration.Core;

public class IntegrationEngine
{
    private readonly IntegrationProviderRegistry _registry;
    private readonly ILogger<IntegrationEngine> _logger;

    public IntegrationEngine(IntegrationProviderRegistry registry, ILogger<IntegrationEngine> logger)
    {
        _registry = registry;
        _logger = logger;
    }

    public async Task<IntegrationResult> ExecuteAsync(string providerKey, string operation, object? parameters = null)
    {
        try
        {
            _logger.LogInformation("Executing integration: {ProviderKey}, Operation: {Operation}", providerKey, operation);

            var provider = _registry.GetProvider<IIntegrationProvider>(providerKey);
            if (provider == null)
            {
                _logger.LogError("Provider not found: {ProviderKey}", providerKey);
                return IntegrationResult.Failure($"Provider '{providerKey}' not found");
            }

            // Test connection first
            var connectionTest = await provider.TestConnectionAsync();
            if (!connectionTest)
            {
                _logger.LogError("Connection test failed for provider: {ProviderKey}", providerKey);
                return IntegrationResult.Failure($"Connection test failed for provider '{providerKey}'");
            }

            // Execute the operation based on provider type and operation
            var result = await ExecuteProviderOperationAsync(provider, operation, parameters);
            
            _logger.LogInformation("Integration execution completed successfully: {ProviderKey}", providerKey);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing integration: {ProviderKey}, Operation: {Operation}", providerKey, operation);
            return IntegrationResult.Failure($"Error: {ex.Message}");
        }
    }

    private async Task<IntegrationResult> ExecuteProviderOperationAsync(IIntegrationProvider provider, string operation, object? parameters)
    {
        return provider switch
        {
            IErpIntegration erpProvider => await ExecuteErpOperationAsync(erpProvider, operation, parameters),
            ICrmIntegration crmProvider => await ExecuteCrmOperationAsync(crmProvider, operation, parameters),
            IShippingProvider shippingProvider => await ExecuteShippingOperationAsync(shippingProvider, operation, parameters),
            IPaymentProvider paymentProvider => await ExecutePaymentOperationAsync(paymentProvider, operation, parameters),
            _ => IntegrationResult.Failure($"Unknown provider type: {provider.GetType().Name}")
        };
    }

    private async Task<IntegrationResult> ExecuteErpOperationAsync(IErpIntegration provider, string operation, object? parameters)
    {
        switch (operation.ToLowerInvariant())
        {
            case "syncorder":
                var orderNumber = parameters?.ToString() ?? string.Empty;
                await provider.SyncOrderAsync(orderNumber);
                return IntegrationResult.Success($"Order {orderNumber} synced successfully");
            
            case "updateinventory":
                var inventoryParams = parameters as dynamic;
                await provider.UpdateInventoryAsync(inventoryParams?.sku, inventoryParams?.quantity);
                return IntegrationResult.Success("Inventory updated successfully");
            
            case "getproduct":
                var sku = parameters?.ToString() ?? string.Empty;
                var product = await provider.GetProductDetailsAsync(sku);
                return IntegrationResult.Success("Product retrieved successfully", product);
            
            default:
                return IntegrationResult.Failure($"Unknown ERP operation: {operation}");
        }
    }

    private async Task<IntegrationResult> ExecuteCrmOperationAsync(ICrmIntegration provider, string operation, object? parameters)
    {
        switch (operation.ToLowerInvariant())
        {
            case "synccustomer":
                var syncParams = parameters as dynamic;
                await provider.SyncCustomerAsync(syncParams?.userId, syncParams?.customerData);
                return IntegrationResult.Success("Customer synced successfully");
            
            case "getcustomer":
                var userId = parameters?.ToString() ?? string.Empty;
                var customer = await provider.GetCustomerDataAsync(userId);
                return IntegrationResult.Success("Customer retrieved successfully", customer);
            
            default:
                return IntegrationResult.Failure($"Unknown CRM operation: {operation}");
        }
    }

    private async Task<IntegrationResult> ExecuteShippingOperationAsync(IShippingProvider provider, string operation, object? parameters)
    {
        switch (operation.ToLowerInvariant())
        {
            case "getrate":
                var rate = await provider.GetShippingRateAsync(parameters ?? new object());
                return IntegrationResult.Success("Shipping rate calculated successfully", rate);
            
            case "bookshipment":
                var bookParams = parameters as dynamic;
                var trackingNumber = await provider.BookShipmentAsync(bookParams?.orderNumber, bookParams?.shippingDetails);
                return IntegrationResult.Success("Shipment booked successfully", trackingNumber);
            
            case "track":
                var trackingNum = parameters?.ToString() ?? string.Empty;
                var tracking = await provider.TrackShipmentAsync(trackingNum);
                return IntegrationResult.Success("Tracking information retrieved successfully", tracking);
            
            default:
                return IntegrationResult.Failure($"Unknown Shipping operation: {operation}");
        }
    }

    private async Task<IntegrationResult> ExecutePaymentOperationAsync(IPaymentProvider provider, string operation, object? parameters)
    {
        switch (operation.ToLowerInvariant())
        {
            case "processpayment":
                var paymentParams = parameters as dynamic;
                var payment = await provider.ProcessPaymentAsync(paymentParams?.amount, paymentParams?.paymentDetails);
                return IntegrationResult.Success("Payment processed successfully", payment);
            
            case "refund":
                var refundParams = parameters as dynamic;
                var refund = await provider.RefundPaymentAsync(refundParams?.transactionId, refundParams?.amount);
                return IntegrationResult.Success("Refund processed successfully", refund);
            
            case "getstatus":
                var transactionId = parameters?.ToString() ?? string.Empty;
                var status = await provider.GetPaymentStatusAsync(transactionId);
                return IntegrationResult.Success("Payment status retrieved successfully", status);
            
            default:
                return IntegrationResult.Failure($"Unknown Payment operation: {operation}");
        }
    }
}

public class IntegrationResult
{
    public bool IsSuccess { get; set; }
    public string Message { get; set; } = string.Empty;
    public object? Data { get; set; }

    public static IntegrationResult Success(string message, object? data = null)
    {
        return new IntegrationResult { IsSuccess = true, Message = message, Data = data };
    }

    public static IntegrationResult Failure(string message)
    {
        return new IntegrationResult { IsSuccess = false, Message = message };
    }
}
