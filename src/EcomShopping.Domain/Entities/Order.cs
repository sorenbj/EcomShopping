namespace EcomShopping.Domain.Entities;

public class Order
{
    public int Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public string? UserId { get; set; }
    public OrderStatus Status { get; set; }
    public decimal SubTotal { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal ShippingAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public int? CouponId { get; set; }
    public string? CouponCode { get; set; }
    public decimal TaxRate { get; set; }
    public int? ShippingAddressId { get; set; }
    public int? BillingAddressId { get; set; }
    public string? PaymentMethod { get; set; }
    public string? PaymentTransactionId { get; set; }
    public PaymentStatus PaymentStatus { get; set; }
    public DateTime OrderDate { get; set; }
    public DateTime? ShippedDate { get; set; }
    public DateTime? DeliveredDate { get; set; }
    public DateTime? CancelledDate { get; set; }

    // Navigation properties
    public Address? ShippingAddress { get; set; }
    public Address? BillingAddress { get; set; }
    public Coupon? Coupon { get; set; }
    public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
}

public enum OrderStatus
{
    Pending = 0,
    Processing = 1,
    Shipped = 2,
    Delivered = 3,
    Cancelled = 4
}

public enum PaymentStatus
{
    Pending = 0,
    Authorized = 1,
    Captured = 2,
    Failed = 3,
    Refunded = 4,
    PartiallyRefunded = 5
}
