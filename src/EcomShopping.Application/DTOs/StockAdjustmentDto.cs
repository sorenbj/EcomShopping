namespace EcomShopping.Application.DTOs;

/// <summary>
/// Data transfer object for stock adjustment
/// </summary>
public class StockAdjustmentDto
{
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public string Type { get; set; } = string.Empty; // Purchase, Sale, Adjustment, Return, Damage
    public string? Reference { get; set; }
    public string? Notes { get; set; }
}
