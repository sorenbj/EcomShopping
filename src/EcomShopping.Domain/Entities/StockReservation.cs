namespace EcomShopping.Domain.Entities;

/// <summary>
/// Represents a temporary stock reservation during checkout
/// </summary>
public class StockReservation
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public string SessionId { get; set; } = string.Empty;
    public string? OrderNumber { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public bool IsReleased { get; set; }
    public DateTime? ReleasedAt { get; set; }

    // Navigation properties
    public Product Product { get; set; } = null!;
}
