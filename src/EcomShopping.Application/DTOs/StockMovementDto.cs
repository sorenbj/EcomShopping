using EcomShopping.Domain.Entities;

namespace EcomShopping.Application.DTOs;

/// <summary>
/// Stock movement data transfer object for API responses
/// </summary>
public class StockMovementDto
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string? ProductName { get; set; }
    public string? ProductSKU { get; set; }
    public int Quantity { get; set; }
    public string Type { get; set; } = string.Empty;
    public string? Reference { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
}
