using EcomShopping.Application.DTOs;
using FluentAssertions;
using Xunit;

namespace EcomShopping.UnitTests.Application;

public class StockDtoTests
{
    [Fact]
    public void StockAdjustmentDto_ShouldSetAllProperties()
    {
        // Arrange & Act
        var dto = new StockAdjustmentDto
        {
            ProductId = 1,
            Quantity = 50,
            Type = "Purchase",
            Reference = "PO-12345",
            Notes = "Restocking inventory"
        };

        // Assert
        dto.ProductId.Should().Be(1);
        dto.Quantity.Should().Be(50);
        dto.Type.Should().Be("Purchase");
        dto.Reference.Should().Be("PO-12345");
        dto.Notes.Should().Be("Restocking inventory");
    }

    [Fact]
    public void StockMovementDto_ShouldIncludeProductInformation()
    {
        // Arrange & Act
        var dto = new StockMovementDto
        {
            Id = 1,
            ProductId = 100,
            ProductName = "Test Product",
            ProductSKU = "TEST-SKU",
            Quantity = 25,
            Type = "Sale",
            CreatedBy = "admin@test.com"
        };

        // Assert
        dto.ProductId.Should().Be(100);
        dto.ProductName.Should().Be("Test Product");
        dto.ProductSKU.Should().Be("TEST-SKU");
        dto.Quantity.Should().Be(25);
        dto.Type.Should().Be("Sale");
        dto.CreatedBy.Should().Be("admin@test.com");
    }
}
