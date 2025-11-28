using EcomShopping.Domain.Entities;
using EcomShopping.Domain.Interfaces;

namespace EcomShopping.Infrastructure.Services;

public class CheckoutService
{
    private readonly ICartRepository _cartRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly IProductRepository _productRepository;
    private readonly ICouponRepository _couponRepository;
    private readonly IPaymentProvider _paymentProvider;
    private readonly IStockReservationRepository _stockReservationRepository;
    private readonly InventoryService _inventoryService;

    public CheckoutService(
        ICartRepository cartRepository,
        IOrderRepository orderRepository,
        IProductRepository productRepository,
        ICouponRepository couponRepository,
        IPaymentProvider paymentProvider,
        IStockReservationRepository stockReservationRepository,
        InventoryService inventoryService)
    {
        _cartRepository = cartRepository;
        _orderRepository = orderRepository;
        _productRepository = productRepository;
        _couponRepository = couponRepository;
        _paymentProvider = paymentProvider;
        _stockReservationRepository = stockReservationRepository;
        _inventoryService = inventoryService;
    }

    public async Task<CheckoutResult> ProcessCheckoutAsync(CheckoutData checkoutData)
    {
        // 1. Get the cart
        var cart = await GetCartAsync(checkoutData.SessionId, checkoutData.UserId);
        if (cart == null || !cart.Items.Any())
        {
            return new CheckoutResult
            {
                Success = false,
                ErrorMessage = "Cart is empty"
            };
        }

        // 2. Validate inventory
        var inventoryCheck = await ValidateInventoryAsync(cart);
        if (!inventoryCheck.Success)
        {
            return inventoryCheck;
        }

        // 3. Calculate order amounts
        var calculation = await CalculateOrderAmountsAsync(cart, checkoutData.CouponCode, checkoutData.TaxRate);
        if (!calculation.Success)
        {
            return new CheckoutResult
            {
                Success = false,
                ErrorMessage = calculation.ErrorMessage
            };
        }

        // 4. Process payment
        PaymentResult? paymentResult = null;
        if (checkoutData.PaymentRequest != null)
        {
            checkoutData.PaymentRequest.Amount = calculation.TotalAmount;
            paymentResult = await _paymentProvider.AuthorizePaymentAsync(checkoutData.PaymentRequest);
            
            if (!paymentResult.Success)
            {
                return new CheckoutResult
                {
                    Success = false,
                    ErrorMessage = $"Payment failed: {paymentResult.ErrorMessage}"
                };
            }
        }

        // 5. Create order
        var order = new Order
        {
            OrderNumber = GenerateOrderNumber(),
            UserId = checkoutData.UserId,
            Status = OrderStatus.Pending,
            PaymentStatus = paymentResult != null ? PaymentStatus.Authorized : PaymentStatus.Pending,
            OrderDate = DateTime.UtcNow,
            SubTotal = calculation.SubTotal,
            DiscountAmount = calculation.DiscountAmount,
            TaxAmount = calculation.TaxAmount,
            ShippingAmount = calculation.ShippingAmount,
            TotalAmount = calculation.TotalAmount,
            TaxRate = checkoutData.TaxRate,
            CouponId = calculation.CouponId,
            CouponCode = checkoutData.CouponCode,
            ShippingAddressId = checkoutData.ShippingAddressId,
            BillingAddressId = checkoutData.BillingAddressId,
            PaymentMethod = checkoutData.PaymentMethod,
            PaymentTransactionId = paymentResult?.TransactionId,
            Items = cart.Items.Select(ci => new OrderItem
            {
                ProductId = ci.ProductId,
                Quantity = ci.Quantity,
                UnitPrice = ci.UnitPrice,
                TotalPrice = ci.Quantity * ci.UnitPrice
            }).ToList()
        };

        var createdOrder = await _orderRepository.AddAsync(order);

        // 6. Reduce inventory
        await ReduceInventoryAsync(cart);

        // 7. Clear the cart
        await _cartRepository.DeleteAsync(cart.Id);

        // 8. Capture payment if authorized
        if (paymentResult != null && paymentResult.Success)
        {
            await _paymentProvider.CapturePaymentAsync(paymentResult.TransactionId!, calculation.TotalAmount);
            createdOrder.PaymentStatus = PaymentStatus.Captured;
            await _orderRepository.UpdateAsync(createdOrder);
        }

        return new CheckoutResult
        {
            Success = true,
            Order = createdOrder
        };
    }

    private async Task<Cart?> GetCartAsync(string sessionId, string? userId)
    {
        var cart = await _cartRepository.GetBySessionIdAsync(sessionId);
        if (cart == null && userId != null)
        {
            cart = await _cartRepository.GetByUserIdAsync(userId);
        }
        return cart;
    }

    private async Task<CheckoutResult> ValidateInventoryAsync(Cart cart)
    {
        foreach (var item in cart.Items)
        {
            var product = await _productRepository.GetByIdAsync(item.ProductId);
            if (product == null)
            {
                return new CheckoutResult
                {
                    Success = false,
                    ErrorMessage = $"Product {item.ProductId} not found"
                };
            }

            // Check available stock (actual stock minus active reservations)
            var availableStock = await _stockReservationRepository.GetAvailableStockAsync(item.ProductId);
            if (availableStock < item.Quantity)
            {
                return new CheckoutResult
                {
                    Success = false,
                    ErrorMessage = $"Insufficient stock for {product.Name}. Only {availableStock} available."
                };
            }
        }

        return new CheckoutResult { Success = true };
    }

    private async Task<OrderCalculation> CalculateOrderAmountsAsync(Cart cart, string? couponCode, decimal taxRate)
    {
        var calculation = new OrderCalculation { Success = true };

        // Calculate subtotal
        calculation.SubTotal = cart.Items.Sum(i => i.Quantity * i.UnitPrice);

        // Apply coupon if provided
        if (!string.IsNullOrWhiteSpace(couponCode))
        {
            var coupon = await _couponRepository.GetByCodeAsync(couponCode);
            if (coupon == null)
            {
                calculation.Success = false;
                calculation.ErrorMessage = "Invalid coupon code";
                return calculation;
            }

            if (!ValidateCoupon(coupon, calculation.SubTotal))
            {
                calculation.Success = false;
                calculation.ErrorMessage = "Coupon is not valid or has expired";
                return calculation;
            }

            calculation.CouponId = coupon.Id;
            calculation.DiscountAmount = CalculateDiscount(coupon, calculation.SubTotal);
        }

        // Calculate shipping (could be enhanced with actual shipping provider)
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
        calculation.TotalAmount = calculation.SubTotal - calculation.DiscountAmount + calculation.ShippingAmount + calculation.TaxAmount;

        return calculation;
    }

    private bool ValidateCoupon(Coupon coupon, decimal orderAmount)
    {
        var now = DateTime.UtcNow;

        // Check if active
        if (!coupon.IsActive)
            return false;

        // Check validity dates
        if (coupon.ValidFrom.HasValue && coupon.ValidFrom.Value > now)
            return false;

        if (coupon.ValidUntil.HasValue && coupon.ValidUntil.Value < now)
            return false;

        // Check usage limit
        if (coupon.UsageLimit.HasValue && coupon.UsageCount >= coupon.UsageLimit.Value)
            return false;

        // Check minimum order amount
        if (coupon.MinimumOrderAmount.HasValue && orderAmount < coupon.MinimumOrderAmount.Value)
            return false;

        return true;
    }

    private decimal CalculateDiscount(Coupon coupon, decimal subTotal)
    {
        decimal discount = 0;

        switch (coupon.Type)
        {
            case CouponType.Percentage:
                discount = Math.Round(subTotal * (coupon.Value / 100), 2);
                break;
            case CouponType.FixedAmount:
                discount = coupon.Value;
                break;
            case CouponType.FreeShipping:
                // Free shipping is handled separately
                discount = 0;
                break;
        }

        // Apply maximum discount cap if set
        if (coupon.MaximumDiscountAmount.HasValue && discount > coupon.MaximumDiscountAmount.Value)
        {
            discount = coupon.MaximumDiscountAmount.Value;
        }

        // Ensure discount doesn't exceed subtotal
        if (discount > subTotal)
        {
            discount = subTotal;
        }

        return discount;
    }

    private decimal CalculateShipping(Cart cart)
    {
        // Simple flat rate shipping calculation
        // Could be enhanced to use actual shipping provider
        var itemCount = cart.Items.Sum(i => i.Quantity);
        
        if (itemCount <= 3)
            return 5.99m;
        else if (itemCount <= 10)
            return 9.99m;
        else
            return 14.99m;
    }

    private async Task ReduceInventoryAsync(Cart cart)
    {
        foreach (var item in cart.Items)
        {
            var product = await _productRepository.GetByIdAsync(item.ProductId);
            if (product != null)
            {
                product.StockQuantity -= item.Quantity;
                await _productRepository.UpdateAsync(product);

                // Check for low stock and create event if needed (using optimized overload)
                var availableStock = await _stockReservationRepository.GetAvailableStockAsync(product.Id);
                await _inventoryService.CheckAndCreateLowStockEventAsync(
                    product.Id, 
                    product.Name, 
                    product.SKU, 
                    availableStock, 
                    product.LowStockThreshold);
            }
        }
    }

    private string GenerateOrderNumber()
    {
        return $"ORD-{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid().ToString("N")[..6].ToUpperInvariant()}";
    }
}

public class CheckoutData
{
    public string SessionId { get; set; } = string.Empty;
    public string? UserId { get; set; }
    public int? ShippingAddressId { get; set; }
    public int? BillingAddressId { get; set; }
    public string? CouponCode { get; set; }
    public decimal TaxRate { get; set; } = 0.0m;
    public string? PaymentMethod { get; set; }
    public PaymentRequest? PaymentRequest { get; set; }
}

public class CheckoutResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public Order? Order { get; set; }
}

public class OrderCalculation
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public decimal SubTotal { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal ShippingAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public int? CouponId { get; set; }
}
