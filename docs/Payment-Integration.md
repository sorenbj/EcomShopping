# Payment Integration Guide

## Overview

The EcomShopping platform includes a payment provider abstraction that allows for easy integration with various payment gateways. The system includes a fake payment provider for testing and development purposes.

## Architecture

The payment system is built using the following components:

### 1. IPaymentProvider Interface

Located in `EcomShopping.Domain/Interfaces/IPaymentProvider.cs`

```csharp
public interface IPaymentProvider
{
    Task<PaymentResult> AuthorizePaymentAsync(PaymentRequest request);
    Task<PaymentResult> CapturePaymentAsync(string transactionId, decimal amount);
    Task<PaymentResult> RefundPaymentAsync(string transactionId, decimal amount);
    Task<PaymentStatusResult> GetPaymentStatusAsync(string transactionId);
}
```

### 2. Payment Models

#### PaymentRequest
```csharp
public class PaymentRequest
{
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "USD";
    public string? CardNumber { get; set; }
    public string? CardHolderName { get; set; }
    public string? ExpiryMonth { get; set; }
    public string? ExpiryYear { get; set; }
    public string? Cvv { get; set; }
    public Dictionary<string, string> Metadata { get; set; } = new();
}
```

#### PaymentResult
```csharp
public class PaymentResult
{
    public bool Success { get; set; }
    public string? TransactionId { get; set; }
    public string? ErrorMessage { get; set; }
    public string? ErrorCode { get; set; }
    public Dictionary<string, string> Metadata { get; set; } = new();
}
```

#### PaymentStatusResult
```csharp
public class PaymentStatusResult
{
    public bool Success { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? TransactionId { get; set; }
    public decimal? Amount { get; set; }
}
```

## Fake Payment Provider

The `FakePaymentProvider` is included for testing and development. It simulates a real payment gateway with the following behavior:

### Authorization

```csharp
var request = new PaymentRequest
{
    Amount = 100.00m,
    CardNumber = "4111111111111111",
    CardHolderName = "John Doe",
    ExpiryMonth = "12",
    ExpiryYear = "2025",
    Cvv = "123"
};

var result = await paymentProvider.AuthorizePaymentAsync(request);
```

**Success Criteria:**
- Card number does NOT end with "0000"
- Returns success with transaction ID

**Failure Simulation:**
- Card number ending with "0000" triggers decline
- Returns error message: "Payment declined by issuing bank"

### Capture

```csharp
var captureResult = await paymentProvider.CapturePaymentAsync(transactionId, 100.00m);
```

**Success Criteria:**
- Transaction must exist
- Transaction status must be "Authorized"
- Capture amount must not exceed authorized amount

### Refund

```csharp
var refundResult = await paymentProvider.RefundPaymentAsync(transactionId, 50.00m);
```

**Success Criteria:**
- Transaction must exist
- Transaction status must be "Captured"
- Refund amount must not exceed captured amount

### Status Check

```csharp
var status = await paymentProvider.GetPaymentStatusAsync(transactionId);
```

Returns the current status of the transaction:
- "Authorized"
- "Captured"
- "Refunded"
- "PartiallyRefunded"

## Integrating a Real Payment Gateway

To integrate with a real payment gateway (e.g., Stripe, PayPal, Square):

### Step 1: Create a New Provider Class

Create a new class implementing `IPaymentProvider`:

```csharp
using EcomShopping.Domain.Interfaces;

namespace EcomShopping.Infrastructure.Payment;

public class StripePaymentProvider : IPaymentProvider
{
    private readonly ILogger<StripePaymentProvider> _logger;
    private readonly string _apiKey;

    public StripePaymentProvider(
        ILogger<StripePaymentProvider> logger,
        IConfiguration configuration)
    {
        _logger = logger;
        _apiKey = configuration["Stripe:ApiKey"] ?? throw new ArgumentNullException();
    }

    public async Task<PaymentResult> AuthorizePaymentAsync(PaymentRequest request)
    {
        try
        {
            // Implement Stripe authorization logic
            // Use Stripe SDK to create payment intent
            
            return new PaymentResult
            {
                Success = true,
                TransactionId = "stripe_transaction_id"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Stripe authorization failed");
            return new PaymentResult
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
    }

    // Implement other methods...
}
```

### Step 2: Register the Provider

In `Program.cs`:

```csharp
// Register payment provider
#if DEBUG
    builder.Services.AddScoped<IPaymentProvider, FakePaymentProvider>();
#else
    builder.Services.AddScoped<IPaymentProvider, StripePaymentProvider>();
#endif
```

### Step 3: Configure Settings

In `appsettings.json`:

```json
{
  "Stripe": {
    "ApiKey": "sk_test_...",
    "PublishableKey": "pk_test_..."
  }
}
```

In `appsettings.Production.json`:

```json
{
  "Stripe": {
    "ApiKey": "sk_live_...",
    "PublishableKey": "pk_live_..."
  }
}
```

## Payment Flow in Checkout

The checkout process integrates with the payment provider as follows:

1. **Validation**: Cart and inventory are validated
2. **Calculation**: Order amounts (subtotal, discount, tax, shipping) are calculated
3. **Authorization**: Payment is authorized via `AuthorizePaymentAsync`
4. **Order Creation**: Order is created with payment transaction ID
5. **Inventory Reduction**: Product stock is reduced
6. **Cart Clearing**: Cart is cleared
7. **Capture**: Payment is captured via `CapturePaymentAsync`
8. **Status Update**: Order payment status is updated to "Captured"

### Checkout Code Example

```csharp
// Process payment
PaymentResult? paymentResult = null;
if (checkoutData.PaymentRequest != null)
{
    checkoutData.PaymentRequest.Amount = calculation.TotalAmount;
    paymentResult = await _paymentProvider.AuthorizePaymentAsync(checkoutData.PaymentRequest);
    
    if (!paymentResult.Success)
    {
        return new CheckoutResult
        {
            Success = false,
            ErrorMessage = $"Payment failed: {paymentResult.ErrorMessage}"
        };
    }
}

// Create order
var order = new Order
{
    // ... order properties
    PaymentStatus = paymentResult != null ? PaymentStatus.Authorized : PaymentStatus.Pending,
    PaymentTransactionId = paymentResult?.TransactionId,
};

var createdOrder = await _orderRepository.AddAsync(order);

// Capture payment if authorized
if (paymentResult != null && paymentResult.Success)
{
    await _paymentProvider.CapturePaymentAsync(paymentResult.TransactionId!, calculation.TotalAmount);
    createdOrder.PaymentStatus = PaymentStatus.Captured;
    await _orderRepository.UpdateAsync(createdOrder);
}
```

## Error Handling

The payment provider should handle and return appropriate errors:

### Common Error Scenarios

1. **Declined Card**
   - ErrorCode: "DECLINED"
   - ErrorMessage: "Payment declined by issuing bank"

2. **Insufficient Funds**
   - ErrorCode: "INSUFFICIENT_FUNDS"
   - ErrorMessage: "Insufficient funds"

3. **Invalid Card**
   - ErrorCode: "INVALID_CARD"
   - ErrorMessage: "Invalid card number"

4. **Expired Card**
   - ErrorCode: "EXPIRED_CARD"
   - ErrorMessage: "Card has expired"

5. **Transaction Not Found**
   - ErrorCode: "NOT_FOUND"
   - ErrorMessage: "Transaction not found"

6. **Amount Exceeded**
   - ErrorCode: "AMOUNT_EXCEEDED"
   - ErrorMessage: "Amount exceeds authorized/captured amount"

## Security Best Practices

### 1. PCI Compliance

**Never store sensitive card data:**
- Do not log card numbers, CVV, or expiry dates
- Use tokenization when possible
- Process payments through PCI-compliant gateways

### 2. Encryption

**Encrypt sensitive data in transit:**
```csharp
// Use HTTPS for all payment communications
builder.Services.AddHttpsRedirection(options =>
{
    options.HttpsPort = 443;
});
```

### 3. API Keys

**Store API keys securely:**
```bash
# Use user secrets for development
dotnet user-secrets set "Stripe:ApiKey" "sk_test_..."

# Use Azure Key Vault or AWS Secrets Manager for production
```

### 4. Idempotency

**Implement idempotency to prevent duplicate charges:**
```csharp
public async Task<PaymentResult> AuthorizePaymentAsync(PaymentRequest request)
{
    var idempotencyKey = request.Metadata.GetValueOrDefault("IdempotencyKey");
    
    // Check if already processed
    var existing = await _cache.GetAsync(idempotencyKey);
    if (existing != null)
    {
        return existing;
    }
    
    // Process payment...
}
```

### 5. Rate Limiting

**Protect against brute force attacks:**
```csharp
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("payment", config =>
    {
        config.Window = TimeSpan.FromMinutes(1);
        config.PermitLimit = 10;
    });
});
```

## Testing

### Unit Tests

Test the payment provider implementation:

```csharp
[Fact]
public async Task AuthorizePayment_WithValidCard_ShouldSucceed()
{
    // Arrange
    var provider = new FakePaymentProvider();
    var request = new PaymentRequest
    {
        Amount = 100.00m,
        CardNumber = "4111111111111111"
    };

    // Act
    var result = await provider.AuthorizePaymentAsync(request);

    // Assert
    result.Success.Should().BeTrue();
    result.TransactionId.Should().NotBeNullOrEmpty();
}
```

### Integration Tests

Test the full checkout flow with payment:

```csharp
[Fact]
public async Task Checkout_WithPayment_ShouldCreateOrderAndProcessPayment()
{
    // Arrange
    var checkoutRequest = new CheckoutRequest
    {
        SessionId = "test-session",
        PaymentDetails = new PaymentDetailsDto
        {
            CardNumber = "4111111111111111",
            CardHolderName = "John Doe",
            ExpiryMonth = "12",
            ExpiryYear = "2025",
            Cvv = "123"
        },
        TaxRate = 0.08m
    };

    // Act
    var response = await _client.PostAsJsonAsync("/api/orders/checkout", checkoutRequest);

    // Assert
    response.StatusCode.Should().Be(HttpStatusCode.OK);
    var result = await response.Content.ReadFromJsonAsync<CheckoutResponse>();
    result.Success.Should().BeTrue();
    result.Order.PaymentStatus.Should().Be(PaymentStatus.Captured);
}
```

## Monitoring and Logging

### Log Payment Events

```csharp
public async Task<PaymentResult> AuthorizePaymentAsync(PaymentRequest request)
{
    _logger.LogInformation(
        "Processing payment authorization for amount {Amount}", 
        request.Amount);

    try
    {
        var result = await ProcessAuthorizationAsync(request);
        
        if (result.Success)
        {
            _logger.LogInformation(
                "Payment authorized successfully. TransactionId: {TransactionId}", 
                result.TransactionId);
        }
        else
        {
            _logger.LogWarning(
                "Payment authorization failed. Error: {ErrorMessage}", 
                result.ErrorMessage);
        }
        
        return result;
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Payment authorization exception");
        throw;
    }
}
```

### Track Payment Metrics

Monitor important payment metrics:
- Authorization success rate
- Capture success rate
- Average transaction amount
- Failed payment reasons
- Processing time

## Supported Payment Providers

### Future Integrations

The system is designed to support multiple payment providers:

- **Stripe**: Credit cards, digital wallets, bank transfers
- **PayPal**: PayPal accounts, credit cards
- **Square**: Credit cards, digital wallets
- **Authorize.Net**: Credit cards, eChecks
- **Braintree**: Credit cards, PayPal, Venmo
- **Adyen**: Global payment methods

Each provider can be implemented by creating a new class that implements `IPaymentProvider`.

## Conclusion

The payment integration system provides a flexible, extensible foundation for processing payments. The fake provider allows for development and testing without real payment processing, while the interface makes it easy to integrate with any payment gateway in production.
