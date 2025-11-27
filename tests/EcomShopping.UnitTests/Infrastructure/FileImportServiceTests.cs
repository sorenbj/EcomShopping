using Xunit;
using FluentAssertions;
using EcomShopping.FileImport.Core;

namespace EcomShopping.UnitTests.Infrastructure;

public class FileImportServiceTests
{
    [Fact]
    public void ApplyFieldMappings_ShouldMapFieldsCorrectly()
    {
        // Arrange
        var parsers = new List<IFileParser>();
        var importers = new List<ITableImporter>();
        var service = new FileImportService(parsers, importers);

        var sourceRecord = new Dictionary<string, object>
        {
            { "ProductName", "Test Product" },
            { "ProductSKU", "TEST001" },
            { "ProductPrice", "99.99" }
        };

        var mappings = new List<FieldMapping>
        {
            new() { SourceField = "ProductName", DestinationField = "Name", IsRequired = true },
            new() { SourceField = "ProductSKU", DestinationField = "SKU", IsRequired = true },
            new() { SourceField = "ProductPrice", DestinationField = "Price", IsRequired = true,
                Transform = obj => Convert.ToDecimal(obj.ToString()) }
        };

        // Use reflection to access private method for testing
        var methodInfo = typeof(FileImportService).GetMethod("ApplyFieldMappings",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        // Act
        var result = methodInfo!.Invoke(service, new object[] { sourceRecord, mappings }) as Dictionary<string, object>;

        // Assert
        result.Should().NotBeNull();
        result!["Name"].Should().Be("Test Product");
        result["SKU"].Should().Be("TEST001");
        result["Price"].Should().Be(99.99m);
    }

    [Fact]
    public void ApplyFieldMappings_ShouldUseDefaultValue_WhenFieldMissing()
    {
        // Arrange
        var parsers = new List<IFileParser>();
        var importers = new List<ITableImporter>();
        var service = new FileImportService(parsers, importers);

        var sourceRecord = new Dictionary<string, object>
        {
            { "Name", "Test Product" }
        };

        var mappings = new List<FieldMapping>
        {
            new() { SourceField = "Name", DestinationField = "Name", IsRequired = true },
            new() { SourceField = "IsActive", DestinationField = "IsActive", IsRequired = false,
                DefaultValue = "true" }
        };

        // Use reflection to access private method
        var methodInfo = typeof(FileImportService).GetMethod("ApplyFieldMappings",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        // Act
        var result = methodInfo!.Invoke(service, new object[] { sourceRecord, mappings }) as Dictionary<string, object>;

        // Assert
        result.Should().NotBeNull();
        result!["Name"].Should().Be("Test Product");
        result["IsActive"].Should().Be("true");
    }

    [Fact]
    public void GetAvailableFields_ShouldReturnUniqueFields()
    {
        // Arrange
        var parsers = new List<IFileParser>();
        var importers = new List<ITableImporter>();
        var service = new FileImportService(parsers, importers);

        var records = new List<Dictionary<string, object>>
        {
            new() { { "Name", "Product 1" }, { "SKU", "SKU001" } },
            new() { { "Name", "Product 2" }, { "SKU", "SKU002" }, { "Price", "99.99" } },
            new() { { "Name", "Product 3" }, { "Description", "Desc" } }
        };

        // Act
        var fields = service.GetAvailableFields(records);

        // Assert
        fields.Should().Contain(new[] { "Name", "SKU", "Price", "Description" });
        fields.Should().HaveCount(4);
    }
}
