using EcomShopping.Application.DTOs;
using FluentAssertions;
using Xunit;

namespace EcomShopping.UnitTests.Application;

public class ProductDtoTests
{
    [Fact]
    public void ProductDto_ShouldMapAllProperties()
    {
        // Arrange & Act
        var dto = new ProductDto
        {
            Id = 1,
            Name = "Test Product",
            Slug = "test-product",
            Description = "Test Description",
            Price = 99.99m,
            SKU = "TEST-001",
            CategoryId = 1,
            CategoryName = "Test Category",
            StockQuantity = 10,
            IsActive = true,
            Images = new List<string> { "image1.jpg", "image2.jpg" },
            Metadata = new Dictionary<string, string> { { "color", "red" }, { "size", "large" } }
        };

        // Assert
        dto.Id.Should().Be(1);
        dto.Name.Should().Be("Test Product");
        dto.Slug.Should().Be("test-product");
        dto.Description.Should().Be("Test Description");
        dto.Price.Should().Be(99.99m);
        dto.SKU.Should().Be("TEST-001");
        dto.CategoryId.Should().Be(1);
        dto.CategoryName.Should().Be("Test Category");
        dto.StockQuantity.Should().Be(10);
        dto.IsActive.Should().BeTrue();
        dto.Images.Should().HaveCount(2);
        dto.Metadata.Should().HaveCount(2);
        dto.Metadata["color"].Should().Be("red");
    }

    [Fact]
    public void CreateProductDto_ShouldSetDefaultIsActiveToTrue()
    {
        // Arrange & Act
        var dto = new CreateProductDto
        {
            Name = "New Product",
            SKU = "NEW-001",
            Price = 49.99m
        };

        // Assert
        dto.IsActive.Should().BeTrue();
    }

    [Fact]
    public void CreateProductDto_ShouldAllowMetadata()
    {
        // Arrange & Act
        var dto = new CreateProductDto
        {
            Name = "Product with Metadata",
            SKU = "META-001",
            Price = 29.99m,
            Metadata = new Dictionary<string, string>
            {
                { "brand", "TestBrand" },
                { "warranty", "2 years" }
            }
        };

        // Assert
        dto.Metadata.Should().HaveCount(2);
        dto.Metadata["brand"].Should().Be("TestBrand");
    }
}
