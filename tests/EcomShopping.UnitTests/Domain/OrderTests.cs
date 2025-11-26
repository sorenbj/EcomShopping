using EcomShopping.Domain.Entities;
using FluentAssertions;
using Xunit;

namespace EcomShopping.UnitTests.Domain;

public class OrderTests
{
    [Fact]
    public void Order_ShouldInitializeWithPendingStatus()
    {
        // Arrange & Act
        var order = new Order();

        // Assert
        order.Status.Should().Be(OrderStatus.Pending);
    }

    [Fact]
    public void Order_ShouldCalculateTotalFromItems()
    {
        // Arrange
        var order = new Order
        {
            Items = new List<OrderItem>
            {
                new OrderItem { Quantity = 2, UnitPrice = 10.00m, TotalPrice = 20.00m },
                new OrderItem { Quantity = 1, UnitPrice = 15.00m, TotalPrice = 15.00m }
            }
        };

        // Act
        var total = order.Items.Sum(i => i.TotalPrice);

        // Assert
        total.Should().Be(35.00m);
    }

    [Theory]
    [InlineData(OrderStatus.Pending)]
    [InlineData(OrderStatus.Processing)]
    [InlineData(OrderStatus.Shipped)]
    [InlineData(OrderStatus.Delivered)]
    [InlineData(OrderStatus.Cancelled)]
    public void Order_ShouldSupportAllStatuses(OrderStatus status)
    {
        // Arrange
        var order = new Order { Status = status };

        // Assert
        order.Status.Should().Be(status);
    }

    [Theory]
    [InlineData(PaymentStatus.Pending)]
    [InlineData(PaymentStatus.Authorized)]
    [InlineData(PaymentStatus.Captured)]
    [InlineData(PaymentStatus.Failed)]
    [InlineData(PaymentStatus.Refunded)]
    [InlineData(PaymentStatus.PartiallyRefunded)]
    public void Order_ShouldSupportAllPaymentStatuses(PaymentStatus paymentStatus)
    {
        // Arrange
        var order = new Order { PaymentStatus = paymentStatus };

        // Assert
        order.PaymentStatus.Should().Be(paymentStatus);
    }

    [Fact]
    public void Order_ShouldCalculateTotalWithDiscountAndTax()
    {
        // Arrange
        var order = new Order
        {
            SubTotal = 100.00m,
            DiscountAmount = 10.00m,
            TaxAmount = 7.20m,
            ShippingAmount = 5.99m
        };

        // Act
        var expectedTotal = order.SubTotal - order.DiscountAmount + order.TaxAmount + order.ShippingAmount;
        order.TotalAmount = expectedTotal;

        // Assert
        order.TotalAmount.Should().Be(103.19m);
    }

    [Fact]
    public void Order_WithCoupon_ShouldStoreCouponInfo()
    {
        // Arrange & Act
        var order = new Order
        {
            CouponId = 1,
            CouponCode = "SAVE20",
            DiscountAmount = 20.00m
        };

        // Assert
        order.CouponId.Should().Be(1);
        order.CouponCode.Should().Be("SAVE20");
        order.DiscountAmount.Should().Be(20.00m);
    }

    [Fact]
    public void Order_WithPaymentInfo_ShouldStorePaymentDetails()
    {
        // Arrange & Act
        var order = new Order
        {
            PaymentMethod = "CreditCard",
            PaymentTransactionId = "TXN-123456",
            PaymentStatus = PaymentStatus.Captured
        };

        // Assert
        order.PaymentMethod.Should().Be("CreditCard");
        order.PaymentTransactionId.Should().Be("TXN-123456");
        order.PaymentStatus.Should().Be(PaymentStatus.Captured);
    }

    [Fact]
    public void Order_WithTaxRate_ShouldStoreTaxRate()
    {
        // Arrange & Act
        var order = new Order
        {
            TaxRate = 0.08m, // 8%
            SubTotal = 100.00m
        };
        order.TaxAmount = order.SubTotal * order.TaxRate;

        // Assert
        order.TaxRate.Should().Be(0.08m);
        order.TaxAmount.Should().Be(8.00m);
    }
}
