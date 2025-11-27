namespace EcomShopping.Domain.Entities;

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string SKU { get; set; } = string.Empty;
    public int? CategoryId { get; set; }
    public int StockQuantity { get; set; }
    public int LowStockThreshold { get; set; } = 10;
    public bool IsActive { get; set; } = true;
    public List<string> Images { get; set; } = new();
    public Dictionary<string, string> Metadata { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    public Category? Category { get; set; }
    public ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    public ICollection<StockMovement> StockMovements { get; set; } = new List<StockMovement>();
    public ICollection<StockReservation> StockReservations { get; set; } = new List<StockReservation>();
    public ICollection<LowStockEvent> LowStockEvents { get; set; } = new List<LowStockEvent>();
}
