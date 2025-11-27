namespace EcomShopping.Application.DTOs;

/// <summary>
/// Data transfer object for ERP stock synchronization
/// </summary>
public class ErpStockSyncDto
{
    public string SKU { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public string? Reference { get; set; }
    public string? Notes { get; set; }
}
