namespace EcomShopping.Application.DTOs;

/// <summary>
/// Data transfer object for acknowledging a low-stock event
/// </summary>
public class AcknowledgeLowStockDto
{
    public string AcknowledgedBy { get; set; } = string.Empty;
    public string? Notes { get; set; }
}
