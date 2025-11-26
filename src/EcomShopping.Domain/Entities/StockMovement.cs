namespace EcomShopping.Domain.Entities;

public class StockMovement
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public StockMovementType Type { get; set; }
    public string? Reference { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }

    // Navigation properties
    public Product Product { get; set; } = null!;
}

public enum StockMovementType
{
    Purchase = 0,
    Sale = 1,
    Adjustment = 2,
    Return = 3,
    Damage = 4
}
