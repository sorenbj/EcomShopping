namespace EcomShopping.Application.DTOs;

/// <summary>
/// Product data transfer object for API responses
/// </summary>
public class ProductDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string SKU { get; set; } = string.Empty;
    public int? CategoryId { get; set; }
    public string? CategoryName { get; set; }
    public int StockQuantity { get; set; }
    public bool IsActive { get; set; }
    public List<string> Images { get; set; } = new();
    public Dictionary<string, string> Metadata { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
