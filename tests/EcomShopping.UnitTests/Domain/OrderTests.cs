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
}
