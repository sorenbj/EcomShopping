namespace EcomShopping.Application.DTOs;

public class CartDto
{
    public int Id { get; set; }
    public string SessionId { get; set; } = string.Empty;
    public string? UserId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public List<CartItemDto> Items { get; set; } = new();
    public decimal TotalAmount => Items.Sum(i => i.SubTotal);
}

public class CartItemDto
{
    public int Id { get; set; }
    public int CartId { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string? ProductSku { get; set; }
    public string? ProductImage { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal SubTotal => Quantity * UnitPrice;
    public DateTime AddedAt { get; set; }
}

public class AddToCartRequest
{
    public string SessionId { get; set; } = string.Empty;
    public string? UserId { get; set; }
    public int ProductId { get; set; }
    public int Quantity { get; set; } = 1;
}

public class UpdateCartItemRequest
{
    public string SessionId { get; set; } = string.Empty;
    public string? UserId { get; set; }
    public int Quantity { get; set; }
}
