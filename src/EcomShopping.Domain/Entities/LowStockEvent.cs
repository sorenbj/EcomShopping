namespace EcomShopping.Domain.Entities;

/// <summary>
/// Represents a low-stock alert event
/// </summary>
public class LowStockEvent
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string ProductSKU { get; set; } = string.Empty;
    public int CurrentStock { get; set; }
    public int Threshold { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsAcknowledged { get; set; }
    public DateTime? AcknowledgedAt { get; set; }
    public string? AcknowledgedBy { get; set; }

    // Navigation properties
    public Product Product { get; set; } = null!;
}
