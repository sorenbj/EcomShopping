namespace EcomShopping.Domain.Entities;

public class Cart
{
    public int Id { get; set; }
    public string SessionId { get; set; } = string.Empty;
    public string? UserId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    public ICollection<CartItem> Items { get; set; } = new List<CartItem>();
}
