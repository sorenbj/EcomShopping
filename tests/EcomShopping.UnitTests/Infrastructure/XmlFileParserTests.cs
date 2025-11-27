using Xunit;
using FluentAssertions;
using EcomShopping.FileImport.Core.Parsers;
using System.Text;

namespace EcomShopping.UnitTests.Infrastructure;

public class XmlFileParserTests
{
    private readonly XmlFileParser _parser;

    public XmlFileParserTests()
    {
        _parser = new XmlFileParser();
    }

    [Fact]
    public void CanParse_ShouldReturnTrue_ForXmlExtension()
    {
        // Act
        var result = _parser.CanParse(".xml");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void CanParse_ShouldReturnFalse_ForNonXmlExtension()
    {
        // Act
        var result1 = _parser.CanParse(".json");
        var result2 = _parser.CanParse(".xlsx");

        // Assert
        result1.Should().BeFalse();
        result2.Should().BeFalse();
    }

    [Fact]
    public async Task ParseAsync_ShouldParseSimpleXml()
    {
        // Arrange
        var xml = @"<?xml version=""1.0""?>
<products>
    <product>
        <Name>Product 1</Name>
        <SKU>SKU001</SKU>
        <Price>99.99</Price>
    </product>
    <product>
        <Name>Product 2</Name>
        <SKU>SKU002</SKU>
        <Price>149.99</Price>
    </product>
</products>";
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(xml));

        // Act
        var result = await _parser.ParseAsync(stream);

        // Assert
        var records = result.ToList();
        records.Should().HaveCount(2);
        records[0]["Name"].Should().Be("Product 1");
        records[0]["SKU"].Should().Be("SKU001");
        records[0]["Price"].Should().Be("99.99");
        records[1]["Name"].Should().Be("Product 2");
    }

    [Fact]
    public async Task ParseAsync_ShouldParseXmlWithAttributes()
    {
        // Arrange
        var xml = @"<?xml version=""1.0""?>
<items>
    <item id=""1"" active=""true"">
        <Name>Item 1</Name>
    </item>
</items>";
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(xml));

        // Act
        var result = await _parser.ParseAsync(stream);

        // Assert
        var records = result.ToList();
        records.Should().HaveCount(1);
        records[0]["id"].Should().Be("1");
        records[0]["active"].Should().Be("true");
        records[0]["Name"].Should().Be("Item 1");
    }

    [Fact]
    public async Task ParseAsync_ShouldReturnEmpty_ForEmptyXml()
    {
        // Arrange
        var xml = @"<?xml version=""1.0""?><root></root>";
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(xml));

        // Act
        var result = await _parser.ParseAsync(stream);

        // Assert
        result.Should().BeEmpty();
    }
}
