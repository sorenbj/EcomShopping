namespace EcomShopping.Application.DTOs;

/// <summary>
/// Data transfer object for stock reservation
/// </summary>
public class StockReservationDto
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string? ProductName { get; set; }
    public string? ProductSKU { get; set; }
    public int Quantity { get; set; }
    public string SessionId { get; set; } = string.Empty;
    public string? OrderNumber { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public bool IsReleased { get; set; }
    public DateTime? ReleasedAt { get; set; }
}
