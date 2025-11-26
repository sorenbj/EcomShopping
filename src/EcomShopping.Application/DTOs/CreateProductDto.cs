namespace EcomShopping.Application.DTOs;

/// <summary>
/// Data transfer object for creating a new product
/// </summary>
public class CreateProductDto
{
    public string Name { get; set; } = string.Empty;
    public string? Slug { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string SKU { get; set; } = string.Empty;
    public int? CategoryId { get; set; }
    public int StockQuantity { get; set; }
    public bool IsActive { get; set; } = true;
    public List<string> Images { get; set; } = new();
    public Dictionary<string, string> Metadata { get; set; } = new();
}
