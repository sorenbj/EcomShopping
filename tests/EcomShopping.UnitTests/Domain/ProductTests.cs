using EcomShopping.Domain.Entities;
using FluentAssertions;
using Xunit;

namespace EcomShopping.UnitTests.Domain;

public class ProductTests
{
    [Fact]
    public void Product_ShouldInitializeWithDefaultValues()
    {
        // Arrange & Act
        var product = new Product();

        // Assert
        product.Name.Should().BeEmpty();
        product.Description.Should().BeEmpty();
        product.SKU.Should().BeEmpty();
        product.Price.Should().Be(0);
        product.StockQuantity.Should().Be(0);
        product.Images.Should().BeEmpty();
    }

    [Fact]
    public void Product_ShouldSetProperties()
    {
        // Arrange
        var product = new Product
        {
            Id = 1,
            Name = "Test Product",
            Description = "Test Description",
            SKU = "TEST-001",
            Price = 99.99m,
            StockQuantity = 10
        };

        // Assert
        product.Id.Should().Be(1);
        product.Name.Should().Be("Test Product");
        product.Description.Should().Be("Test Description");
        product.SKU.Should().Be("TEST-001");
        product.Price.Should().Be(99.99m);
        product.StockQuantity.Should().Be(10);
    }

    [Fact]
    public void Product_ShouldAllowMultipleImages()
    {
        // Arrange
        var product = new Product
        {
            Images = new List<string> { "image1.jpg", "image2.jpg", "image3.jpg" }
        };

        // Assert
        product.Images.Should().HaveCount(3);
        product.Images.Should().Contain("image1.jpg");
    }
}
