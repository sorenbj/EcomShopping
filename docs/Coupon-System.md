# Coupon and Discount System Guide

## Overview

The EcomShopping platform includes a comprehensive coupon and discount system that supports multiple discount types, validation rules, and usage tracking.

## Coupon Types

The system supports three types of coupons:

### 1. Percentage Discount

Applies a percentage discount to the order subtotal.

```json
{
  "code": "SAVE20",
  "description": "20% off your entire order",
  "type": 0,
  "value": 20.0,
  "minimumOrderAmount": 50.0,
  "maximumDiscountAmount": 25.0
}
```

**Calculation:**
```
discountAmount = subtotal × (value / 100)
discountAmount = min(discountAmount, maximumDiscountAmount)
discountAmount = min(discountAmount, subtotal)
```

**Example:**
- Subtotal: $100.00
- Discount: 20% = $20.00
- Maximum Discount: $25.00
- Final Discount: $20.00

### 2. Fixed Amount Discount

Applies a fixed dollar amount discount to the order.

```json
{
  "code": "GET10OFF",
  "description": "Get $10 off your order",
  "type": 1,
  "value": 10.0,
  "minimumOrderAmount": 30.0
}
```

**Calculation:**
```
discountAmount = value
discountAmount = min(discountAmount, subtotal)
```

**Example:**
- Subtotal: $50.00
- Discount: $10.00
- Final Discount: $10.00

### 3. Free Shipping

Waives the shipping cost for the order.

```json
{
  "code": "FREESHIP",
  "description": "Free shipping on your order",
  "type": 2,
  "minimumOrderAmount": 25.0
}
```

**Calculation:**
```
shippingAmount = 0
```

**Example:**
- Subtotal: $30.00
- Original Shipping: $5.99
- Final Shipping: $0.00

## Coupon Properties

### Required Fields

- **Code**: Unique coupon identifier (e.g., "SAVE20")
- **Description**: Human-readable description of the discount
- **Type**: CouponType enum (Percentage, FixedAmount, FreeShipping)
- **Value**: Discount value (percentage or dollar amount)

### Optional Constraints

- **MinimumOrderAmount**: Minimum order subtotal required to use coupon
- **MaximumDiscountAmount**: Cap on discount amount for percentage coupons
- **ValidFrom**: Start date for coupon validity
- **ValidUntil**: End date for coupon validity
- **UsageLimit**: Maximum number of times coupon can be used
- **IsActive**: Whether coupon is currently active

### Tracking Fields

- **UsageCount**: Number of times coupon has been used
- **CreatedAt**: When coupon was created
- **UpdatedAt**: When coupon was last modified

## Creating Coupons

### Via API

```http
POST /api/coupons
Content-Type: application/json

{
  "code": "WELCOME10",
  "description": "Welcome bonus - 10% off first order",
  "type": 0,
  "value": 10.0,
  "minimumOrderAmount": 0,
  "maximumDiscountAmount": 50.0,
  "validFrom": "2024-01-01T00:00:00Z",
  "validUntil": "2024-12-31T23:59:59Z",
  "usageLimit": 1000
}
```

### Via Code

```csharp
var coupon = new Coupon
{
    Code = "WELCOME10",
    Description = "Welcome bonus - 10% off first order",
    Type = CouponType.Percentage,
    Value = 10.0m,
    MinimumOrderAmount = 0,
    MaximumDiscountAmount = 50.0m,
    ValidFrom = new DateTime(2024, 1, 1),
    ValidUntil = new DateTime(2024, 12, 31, 23, 59, 59),
    UsageLimit = 1000,
    IsActive = true
};

await _couponRepository.AddAsync(coupon);
```

## Validating Coupons

### Validation Rules

A coupon is considered valid if ALL of the following are true:

1. **Active**: `IsActive == true`
2. **Not Started**: `ValidFrom == null OR ValidFrom <= now`
3. **Not Expired**: `ValidUntil == null OR ValidUntil >= now`
4. **Usage Available**: `UsageLimit == null OR UsageCount < UsageLimit`
5. **Minimum Met**: `MinimumOrderAmount == null OR orderAmount >= MinimumOrderAmount`

### Validation API

```http
POST /api/coupons/validate
Content-Type: application/json

{
  "code": "SAVE20",
  "orderAmount": 100.0
}
```

**Success Response:**
```json
{
  "isValid": true,
  "coupon": {
    "id": 1,
    "code": "SAVE20",
    "type": 0,
    "value": 20.0
  },
  "discountAmount": 20.0
}
```

**Error Response:**
```json
{
  "isValid": false,
  "errorMessage": "Minimum order amount of $50.00 required"
}
```

### Common Validation Errors

| Error | Reason |
|-------|--------|
| "Coupon code not found" | Invalid or non-existent code |
| "This coupon is no longer active" | IsActive = false |
| "This coupon is not valid until {date}" | Before ValidFrom date |
| "This coupon has expired" | After ValidUntil date |
| "This coupon has reached its usage limit" | UsageCount >= UsageLimit |
| "Minimum order amount of {amount} required" | Order below MinimumOrderAmount |

## Using Coupons in Checkout

### Checkout Request with Coupon

```http
POST /api/orders/checkout
Content-Type: application/json

{
  "sessionId": "session-123",
  "couponCode": "SAVE20",
  "taxRate": 0.08,
  "shippingAddress": { ... },
  "paymentDetails": { ... }
}
```

### Discount Calculation Flow

1. **Validate Coupon**: Check if coupon code is valid
2. **Calculate Subtotal**: Sum all cart item prices
3. **Apply Discount**: Calculate discount based on type
4. **Calculate Shipping**: Determine shipping cost
5. **Apply Free Shipping**: If coupon type is FreeShipping
6. **Calculate Tax**: Apply tax to (subtotal - discount + shipping)
7. **Calculate Total**: subtotal - discount + shipping + tax

### Example Calculation

```
Cart Items:
  - Item 1: 2 × $25.00 = $50.00
  - Item 2: 1 × $30.00 = $30.00

Subtotal:           $80.00
Coupon (SAVE20):   -$16.00  (20% of $80.00)
Shipping:           $5.99
Taxable Amount:    $69.99   ($80.00 - $16.00 + $5.99)
Tax (8%):          $5.60
─────────────────────────
Total:             $75.59
```

### Code Example

```csharp
// In CheckoutService
private async Task<OrderCalculation> CalculateOrderAmountsAsync(
    Cart cart, 
    string? couponCode, 
    decimal taxRate)
{
    var calculation = new OrderCalculation { Success = true };

    // Calculate subtotal
    calculation.SubTotal = cart.Items.Sum(i => i.Quantity * i.UnitPrice);

    // Apply coupon if provided
    if (!string.IsNullOrWhiteSpace(couponCode))
    {
        var coupon = await _couponRepository.GetByCodeAsync(couponCode);
        if (coupon == null || !ValidateCoupon(coupon, calculation.SubTotal))
        {
            calculation.Success = false;
            calculation.ErrorMessage = "Invalid or expired coupon";
            return calculation;
        }

        calculation.CouponId = coupon.Id;
        calculation.DiscountAmount = CalculateDiscount(coupon, calculation.SubTotal);
    }

    // Calculate shipping
    calculation.ShippingAmount = CalculateShipping(cart);

    // Apply free shipping if applicable
    if (calculation.CouponId.HasValue)
    {
        var coupon = await _couponRepository.GetByIdAsync(calculation.CouponId.Value);
        if (coupon?.Type == CouponType.FreeShipping)
        {
            calculation.ShippingAmount = 0;
        }
    }

    // Calculate tax
    var taxableAmount = calculation.SubTotal - calculation.DiscountAmount + calculation.ShippingAmount;
    calculation.TaxAmount = Math.Round(taxableAmount * taxRate, 2);

    // Calculate total
    calculation.TotalAmount = calculation.SubTotal - calculation.DiscountAmount 
        + calculation.ShippingAmount + calculation.TaxAmount;

    return calculation;
}
```

## Managing Coupons

### Get All Coupons

```http
GET /api/coupons
```

Returns all coupons in the system (including inactive and expired).

### Get Active Coupons

```http
GET /api/coupons/active
```

Returns only currently valid coupons (active, within date range, not at usage limit).

### Update Coupon

```http
PUT /api/coupons/{id}
Content-Type: application/json

{
  "code": "SAVE20",
  "description": "Updated description",
  "type": 0,
  "value": 25.0,
  "minimumOrderAmount": 50.0,
  "maximumDiscountAmount": 30.0,
  "validFrom": "2024-01-01T00:00:00Z",
  "validUntil": "2024-12-31T23:59:59Z",
  "usageLimit": 500
}
```

### Deactivate Coupon

```http
PUT /api/coupons/{id}
Content-Type: application/json

{
  "code": "SAVE20",
  "isActive": false,
  ...
}
```

### Delete Coupon

```http
DELETE /api/coupons/{id}
```

**Warning**: Deleting a coupon will prevent viewing it in historical orders.

## Best Practices

### 1. Unique Codes

Always use unique, meaningful coupon codes:

✅ Good:
- `WELCOME10`
- `SUMMER20`
- `FREESHIP99`

❌ Bad:
- `123`
- `DISCOUNT`
- `ABC`

### 2. Clear Descriptions

Provide clear, customer-facing descriptions:

✅ Good:
- "20% off your entire order"
- "Get $10 off orders over $50"
- "Free shipping on orders over $25"

❌ Bad:
- "Discount"
- "Promo"
- "Code"

### 3. Set Limits

Always set appropriate limits to prevent abuse:

```json
{
  "usageLimit": 100,
  "minimumOrderAmount": 25.0,
  "maximumDiscountAmount": 50.0,
  "validUntil": "2024-12-31T23:59:59Z"
}
```

### 4. Monitor Usage

Track coupon usage to understand effectiveness:

```csharp
var topCoupons = await _couponRepository.GetAllAsync()
    .OrderByDescending(c => c.UsageCount)
    .Take(10);
```

### 5. Test Before Launch

Always test coupons before making them public:

```csharp
[Fact]
public async Task Coupon_WithMinimumAmount_ShouldValidateCorrectly()
{
    // Arrange
    var coupon = new Coupon
    {
        Code = "TEST50",
        Type = CouponType.Percentage,
        Value = 10,
        MinimumOrderAmount = 50.0m,
        IsActive = true
    };

    // Act - Below minimum
    var isValid1 = ValidateCoupon(coupon, 40.0m);
    // Act - At minimum
    var isValid2 = ValidateCoupon(coupon, 50.0m);
    // Act - Above minimum
    var isValid3 = ValidateCoupon(coupon, 60.0m);

    // Assert
    isValid1.Should().BeFalse();
    isValid2.Should().BeTrue();
    isValid3.Should().BeTrue();
}
```

## Common Scenarios

### Scenario 1: Holiday Sale

20% off everything for Black Friday, limited to first 1000 customers:

```json
{
  "code": "BLACKFRIDAY24",
  "description": "Black Friday Special - 20% off everything!",
  "type": 0,
  "value": 20.0,
  "maximumDiscountAmount": 100.0,
  "validFrom": "2024-11-29T00:00:00Z",
  "validUntil": "2024-11-30T23:59:59Z",
  "usageLimit": 1000
}
```

### Scenario 2: Welcome Discount

$10 off first order:

```json
{
  "code": "WELCOME10",
  "description": "Welcome! Get $10 off your first order",
  "type": 1,
  "value": 10.0,
  "minimumOrderAmount": 30.0
}
```

### Scenario 3: Free Shipping Threshold

Free shipping on orders over $50:

```json
{
  "code": "SHIP50",
  "description": "Free shipping on orders over $50",
  "type": 2,
  "minimumOrderAmount": 50.0
}
```

### Scenario 4: VIP Customer Discount

25% off for VIP customers, max $50 discount:

```json
{
  "code": "VIP25",
  "description": "VIP Customer - 25% off",
  "type": 0,
  "value": 25.0,
  "maximumDiscountAmount": 50.0,
  "minimumOrderAmount": 100.0,
  "validFrom": "2024-01-01T00:00:00Z",
  "validUntil": "2024-12-31T23:59:59Z",
  "usageLimit": 100
}
```

## Database Schema

### Coupon Table

```sql
CREATE TABLE Coupons (
    Id INT PRIMARY KEY IDENTITY,
    Code NVARCHAR(50) NOT NULL UNIQUE,
    Description NVARCHAR(500),
    Type INT NOT NULL,
    Value DECIMAL(18,2) NOT NULL,
    MinimumOrderAmount DECIMAL(18,2) NULL,
    MaximumDiscountAmount DECIMAL(18,2) NULL,
    ValidFrom DATETIME2 NULL,
    ValidUntil DATETIME2 NULL,
    UsageLimit INT NULL,
    UsageCount INT NOT NULL DEFAULT 0,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL,
    UpdatedAt DATETIME2 NULL
);

CREATE INDEX IX_Coupons_Code ON Coupons(Code);
CREATE INDEX IX_Coupons_IsActive ON Coupons(IsActive);
```

### Order Table (with Coupon relation)

```sql
CREATE TABLE Orders (
    Id INT PRIMARY KEY IDENTITY,
    -- ... other columns
    CouponId INT NULL,
    CouponCode NVARCHAR(50) NULL,
    DiscountAmount DECIMAL(18,2) NOT NULL DEFAULT 0,
    -- ... other columns
    FOREIGN KEY (CouponId) REFERENCES Coupons(Id)
);
```

## Future Enhancements

### 1. User-Specific Coupons

Track which users have used which coupons:

```csharp
public class CouponUsage
{
    public int Id { get; set; }
    public int CouponId { get; set; }
    public string UserId { get; set; }
    public DateTime UsedAt { get; set; }
    public int OrderId { get; set; }
}
```

### 2. Product-Specific Coupons

Apply discount to specific products or categories:

```csharp
public class Coupon
{
    // ... existing properties
    public List<int>? ApplicableProductIds { get; set; }
    public List<int>? ApplicableCategoryIds { get; set; }
}
```

### 3. Stackable Coupons

Allow multiple coupons per order:

```csharp
public class Order
{
    // ... existing properties
    public List<int> CouponIds { get; set; }
}
```

### 4. Tiered Discounts

Apply different discount rates at different thresholds:

```csharp
public class TieredCoupon
{
    public List<DiscountTier> Tiers { get; set; }
}

public class DiscountTier
{
    public decimal MinimumAmount { get; set; }
    public decimal DiscountPercentage { get; set; }
}
```

## Conclusion

The coupon and discount system provides flexible, powerful discounting capabilities while maintaining control through validation rules and usage limits. It seamlessly integrates with the checkout process to provide accurate pricing and order totals.
