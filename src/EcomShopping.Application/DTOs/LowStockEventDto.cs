namespace EcomShopping.Application.DTOs;

/// <summary>
/// Data transfer object for low-stock event
/// </summary>
public class LowStockEventDto
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
}
