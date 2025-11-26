using EcomShopping.Domain.Entities;

namespace EcomShopping.Application.DTOs;

public class CouponDto
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public CouponType Type { get; set; }
    public decimal Value { get; set; }
    public decimal? MinimumOrderAmount { get; set; }
    public decimal? MaximumDiscountAmount { get; set; }
    public DateTime? ValidFrom { get; set; }
    public DateTime? ValidUntil { get; set; }
    public int? UsageLimit { get; set; }
    public int UsageCount { get; set; }
    public bool IsActive { get; set; }
}

public class CreateCouponDto
{
    public string Code { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public CouponType Type { get; set; }
    public decimal Value { get; set; }
    public decimal? MinimumOrderAmount { get; set; }
    public decimal? MaximumDiscountAmount { get; set; }
    public DateTime? ValidFrom { get; set; }
    public DateTime? ValidUntil { get; set; }
    public int? UsageLimit { get; set; }
}

public class ValidateCouponRequest
{
    public string Code { get; set; } = string.Empty;
    public decimal OrderAmount { get; set; }
}

public class ValidateCouponResponse
{
    public bool IsValid { get; set; }
    public string? ErrorMessage { get; set; }
    public CouponDto? Coupon { get; set; }
    public decimal DiscountAmount { get; set; }
}
