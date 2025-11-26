using EcomShopping.Domain.Interfaces;
using EcomShopping.Infrastructure.Payment;
using FluentAssertions;
using Xunit;

namespace EcomShopping.UnitTests.Infrastructure;

public class FakePaymentProviderTests
{
    private readonly FakePaymentProvider _paymentProvider;

    public FakePaymentProviderTests()
    {
        _paymentProvider = new FakePaymentProvider();
    }

    [Fact]
    public async Task AuthorizePayment_WithValidCard_ShouldSucceed()
    {
        // Arrange
        var request = new PaymentRequest
        {
            Amount = 100.00m,
            CardNumber = "4111111111111111",
            CardHolderName = "John Doe",
            ExpiryMonth = "12",
            ExpiryYear = "2025",
            Cvv = "123"
        };

        // Act
        var result = await _paymentProvider.AuthorizePaymentAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.TransactionId.Should().NotBeNullOrEmpty();
        result.TransactionId.Should().StartWith("TXN-");
        result.ErrorMessage.Should().BeNullOrEmpty();
    }

    [Fact]
    public async Task AuthorizePayment_WithDeclinedCard_ShouldFail()
    {
        // Arrange
        var request = new PaymentRequest
        {
            Amount = 100.00m,
            CardNumber = "4111111111110000", // Ends with 0000 to trigger decline
            CardHolderName = "John Doe",
            ExpiryMonth = "12",
            ExpiryYear = "2025",
            Cvv = "123"
        };

        // Act
        var result = await _paymentProvider.AuthorizePaymentAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Be("Payment declined by issuing bank");
        result.ErrorCode.Should().Be("DECLINED");
        result.TransactionId.Should().BeNullOrEmpty();
    }

    [Fact]
    public async Task CapturePayment_WithValidTransaction_ShouldSucceed()
    {
        // Arrange
        var authRequest = new PaymentRequest
        {
            Amount = 100.00m,
            CardNumber = "4111111111111111"
        };
        var authResult = await _paymentProvider.AuthorizePaymentAsync(authRequest);

        // Act
        var captureResult = await _paymentProvider.CapturePaymentAsync(authResult.TransactionId!, 100.00m);

        // Assert
        captureResult.Should().NotBeNull();
        captureResult.Success.Should().BeTrue();
        captureResult.TransactionId.Should().Be(authResult.TransactionId);
    }

    [Fact]
    public async Task CapturePayment_WithInvalidTransaction_ShouldFail()
    {
        // Arrange
        var invalidTransactionId = "INVALID-TXN-ID";

        // Act
        var result = await _paymentProvider.CapturePaymentAsync(invalidTransactionId, 100.00m);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Be("Transaction not found");
        result.ErrorCode.Should().Be("NOT_FOUND");
    }

    [Fact]
    public async Task CapturePayment_WithAmountExceedingAuthorization_ShouldFail()
    {
        // Arrange
        var authRequest = new PaymentRequest
        {
            Amount = 100.00m,
            CardNumber = "4111111111111111"
        };
        var authResult = await _paymentProvider.AuthorizePaymentAsync(authRequest);

        // Act
        var captureResult = await _paymentProvider.CapturePaymentAsync(authResult.TransactionId!, 150.00m);

        // Assert
        captureResult.Should().NotBeNull();
        captureResult.Success.Should().BeFalse();
        captureResult.ErrorMessage.Should().Be("Capture amount exceeds authorized amount");
        captureResult.ErrorCode.Should().Be("AMOUNT_EXCEEDED");
    }

    [Fact]
    public async Task RefundPayment_WithCapturedPayment_ShouldSucceed()
    {
        // Arrange
        var authRequest = new PaymentRequest
        {
            Amount = 100.00m,
            CardNumber = "4111111111111111"
        };
        var authResult = await _paymentProvider.AuthorizePaymentAsync(authRequest);
        await _paymentProvider.CapturePaymentAsync(authResult.TransactionId!, 100.00m);

        // Act
        var refundResult = await _paymentProvider.RefundPaymentAsync(authResult.TransactionId!, 100.00m);

        // Assert
        refundResult.Should().NotBeNull();
        refundResult.Success.Should().BeTrue();
        refundResult.TransactionId.Should().Be(authResult.TransactionId);
    }

    [Fact]
    public async Task RefundPayment_WithInvalidTransaction_ShouldFail()
    {
        // Arrange
        var invalidTransactionId = "INVALID-TXN-ID";

        // Act
        var result = await _paymentProvider.RefundPaymentAsync(invalidTransactionId, 50.00m);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Be("Transaction not found");
        result.ErrorCode.Should().Be("NOT_FOUND");
    }

    [Fact]
    public async Task GetPaymentStatus_WithValidTransaction_ShouldReturnStatus()
    {
        // Arrange
        var authRequest = new PaymentRequest
        {
            Amount = 100.00m,
            CardNumber = "4111111111111111"
        };
        var authResult = await _paymentProvider.AuthorizePaymentAsync(authRequest);

        // Act
        var statusResult = await _paymentProvider.GetPaymentStatusAsync(authResult.TransactionId!);

        // Assert
        statusResult.Should().NotBeNull();
        statusResult.Success.Should().BeTrue();
        statusResult.Status.Should().Be("Authorized");
        statusResult.TransactionId.Should().Be(authResult.TransactionId);
        statusResult.Amount.Should().Be(100.00m);
    }

    [Fact]
    public async Task GetPaymentStatus_WithInvalidTransaction_ShouldReturnNotFound()
    {
        // Arrange
        var invalidTransactionId = "INVALID-TXN-ID";

        // Act
        var result = await _paymentProvider.GetPaymentStatusAsync(invalidTransactionId);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Status.Should().Be("NotFound");
    }

    [Fact]
    public async Task PaymentWorkflow_FullCycle_ShouldWork()
    {
        // Arrange
        var authRequest = new PaymentRequest
        {
            Amount = 150.00m,
            CardNumber = "4111111111111111",
            CardHolderName = "Jane Smith"
        };

        // Act & Assert - Authorize
        var authResult = await _paymentProvider.AuthorizePaymentAsync(authRequest);
        authResult.Success.Should().BeTrue();

        // Act & Assert - Check status
        var statusResult = await _paymentProvider.GetPaymentStatusAsync(authResult.TransactionId!);
        statusResult.Status.Should().Be("Authorized");

        // Act & Assert - Capture
        var captureResult = await _paymentProvider.CapturePaymentAsync(authResult.TransactionId!, 150.00m);
        captureResult.Success.Should().BeTrue();

        // Act & Assert - Check status after capture
        statusResult = await _paymentProvider.GetPaymentStatusAsync(authResult.TransactionId!);
        statusResult.Status.Should().Be("Captured");
    }
}
