using EcomShopping.Domain.Entities;
using FluentAssertions;
using Xunit;

namespace EcomShopping.UnitTests.Domain;

public class CouponTests
{
    [Fact]
    public void Coupon_SetProperties_ShouldUpdateValues()
    {
        // Arrange
        var coupon = new Coupon();

        // Act
        coupon.Code = "SAVE20";
        coupon.Description = "20% off your order";
        coupon.Type = CouponType.Percentage;
        coupon.Value = 20;
        coupon.MinimumOrderAmount = 50;
        coupon.IsActive = true;

        // Assert
        coupon.Code.Should().Be("SAVE20");
        coupon.Description.Should().Be("20% off your order");
        coupon.Type.Should().Be(CouponType.Percentage);
        coupon.Value.Should().Be(20);
        coupon.MinimumOrderAmount.Should().Be(50);
        coupon.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Coupon_PercentageType_ShouldHaveCorrectType()
    {
        // Arrange & Act
        var coupon = new Coupon
        {
            Type = CouponType.Percentage,
            Value = 15
        };

        // Assert
        coupon.Type.Should().Be(CouponType.Percentage);
        coupon.Value.Should().Be(15);
    }

    [Fact]
    public void Coupon_FixedAmountType_ShouldHaveCorrectType()
    {
        // Arrange & Act
        var coupon = new Coupon
        {
            Type = CouponType.FixedAmount,
            Value = 10.00m
        };

        // Assert
        coupon.Type.Should().Be(CouponType.FixedAmount);
        coupon.Value.Should().Be(10.00m);
    }

    [Fact]
    public void Coupon_FreeShippingType_ShouldHaveCorrectType()
    {
        // Arrange & Act
        var coupon = new Coupon
        {
            Type = CouponType.FreeShipping,
            Code = "FREESHIP"
        };

        // Assert
        coupon.Type.Should().Be(CouponType.FreeShipping);
        coupon.Code.Should().Be("FREESHIP");
    }

    [Fact]
    public void Coupon_WithValidityDates_ShouldStoreCorrectly()
    {
        // Arrange
        var validFrom = DateTime.UtcNow;
        var validUntil = DateTime.UtcNow.AddDays(30);

        // Act
        var coupon = new Coupon
        {
            ValidFrom = validFrom,
            ValidUntil = validUntil
        };

        // Assert
        coupon.ValidFrom.Should().Be(validFrom);
        coupon.ValidUntil.Should().Be(validUntil);
    }

    [Fact]
    public void Coupon_WithUsageLimit_ShouldTrackUsageCount()
    {
        // Arrange & Act
        var coupon = new Coupon
        {
            UsageLimit = 100,
            UsageCount = 0
        };

        coupon.UsageCount++;

        // Assert
        coupon.UsageLimit.Should().Be(100);
        coupon.UsageCount.Should().Be(1);
    }

    [Fact]
    public void Coupon_DefaultValues_ShouldBeCorrect()
    {
        // Arrange & Act
        var coupon = new Coupon();

        // Assert
        coupon.Code.Should().Be(string.Empty);
        coupon.Description.Should().Be(string.Empty);
        coupon.IsActive.Should().BeTrue();
        coupon.UsageCount.Should().Be(0);
    }

    [Fact]
    public void Coupon_WithMaximumDiscount_ShouldStoreCap()
    {
        // Arrange & Act
        var coupon = new Coupon
        {
            Type = CouponType.Percentage,
            Value = 50,
            MaximumDiscountAmount = 25.00m
        };

        // Assert
        coupon.MaximumDiscountAmount.Should().Be(25.00m);
    }
}
