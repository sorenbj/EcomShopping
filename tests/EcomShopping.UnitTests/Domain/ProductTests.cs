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
        product.Slug.Should().BeEmpty();
        product.Description.Should().BeEmpty();
        product.SKU.Should().BeEmpty();
        product.Price.Should().Be(0);
        product.StockQuantity.Should().Be(0);
        product.IsActive.Should().BeTrue();
        product.Images.Should().BeEmpty();
        product.Metadata.Should().BeEmpty();
    }

    [Fact]
    public void Product_ShouldSetProperties()
    {
        // Arrange
        var product = new Product
        {
            Id = 1,
            Name = "Test Product",
            Slug = "test-product",
            Description = "Test Description",
            SKU = "TEST-001",
            Price = 99.99m,
            StockQuantity = 10,
            IsActive = true
        };

        // Assert
        product.Id.Should().Be(1);
        product.Name.Should().Be("Test Product");
        product.Slug.Should().Be("test-product");
        product.Description.Should().Be("Test Description");
        product.SKU.Should().Be("TEST-001");
        product.Price.Should().Be(99.99m);
        product.StockQuantity.Should().Be(10);
        product.IsActive.Should().BeTrue();
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

    [Fact]
    public void Product_ShouldAllowMetadata()
    {
        // Arrange
        var product = new Product
        {
            Metadata = new Dictionary<string, string>
            {
                { "color", "blue" },
                { "material", "cotton" },
                { "brand", "TestBrand" }
            }
        };

        // Assert
        product.Metadata.Should().HaveCount(3);
        product.Metadata["color"].Should().Be("blue");
        product.Metadata["material"].Should().Be("cotton");
    }

    [Fact]
    public void Product_ShouldAllowInactiveStatus()
    {
        // Arrange
        var product = new Product
        {
            Name = "Inactive Product",
            IsActive = false
        };

        // Assert
        product.IsActive.Should().BeFalse();
    }
}
