using EcomShopping.Application.DTOs;
using EcomShopping.Domain.Entities;
using EcomShopping.Domain.Interfaces;
using EcomShopping.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;

namespace EcomShopping.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly IOrderRepository _orderRepository;
    private readonly CheckoutService _checkoutService;
    private readonly ILogger<OrdersController> _logger;

    public OrdersController(
        IOrderRepository orderRepository,
        CheckoutService checkoutService,
        ILogger<OrdersController> logger)
    {
        _orderRepository = orderRepository;
        _checkoutService = checkoutService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Order>>> GetOrders(
        [FromQuery] string? userId, 
        [FromQuery] OrderStatus? status,
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate,
        [FromQuery] string? orderNumber,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        try
        {
            // Use repository method for database-level filtering and pagination
            var (items, totalCount) = await _orderRepository.GetFilteredOrdersAsync(
                userId, status, startDate, endDate, orderNumber, page, pageSize);

            var orderDtos = items.Select(MapToOrderDto).ToList();

            return Ok(new
            {
                items = orderDtos,
                totalCount,
                page,
                pageSize,
                totalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving orders");
            return StatusCode(500, "An error occurred while retrieving orders");
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Order>> GetOrder(int id)
    {
        try
        {
            var order = await _orderRepository.GetByIdAsync(id);
            if (order == null)
            {
                return NotFound();
            }
            
            var orderDto = MapToOrderDto(order);
            return Ok(orderDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving order {OrderId}", id);
            return StatusCode(500, "An error occurred while retrieving the order");
        }
    }

    [HttpPost("checkout")]
    public async Task<ActionResult<CheckoutResponse>> Checkout([FromBody] CheckoutRequest request)
    {
        try
        {
            // Map AddressDto to Address entity and save if needed
            Address? shippingAddress = null;
            Address? billingAddress = null;

            if (request.ShippingAddress.Id == 0)
            {
                shippingAddress = MapToAddress(request.ShippingAddress);
            }

            if (request.UseSameAddressForBilling)
            {
                billingAddress = shippingAddress;
            }
            else if (request.BillingAddress != null && request.BillingAddress.Id == 0)
            {
                billingAddress = MapToAddress(request.BillingAddress);
            }

            var checkoutData = new CheckoutData
            {
                SessionId = request.SessionId,
                UserId = request.UserId,
                ShippingAddressId = request.ShippingAddress.Id > 0 ? request.ShippingAddress.Id : null,
                BillingAddressId = request.BillingAddress?.Id > 0 ? request.BillingAddress.Id : null,
                CouponCode = request.CouponCode,
                TaxRate = request.TaxRate,
                PaymentMethod = request.PaymentMethod,
                PaymentRequest = request.PaymentDetails != null ? new PaymentRequest
                {
                    CardNumber = request.PaymentDetails.CardNumber,
                    CardHolderName = request.PaymentDetails.CardHolderName,
                    ExpiryMonth = request.PaymentDetails.ExpiryMonth,
                    ExpiryYear = request.PaymentDetails.ExpiryYear,
                    Cvv = request.PaymentDetails.Cvv
                } : null
            };

            var result = await _checkoutService.ProcessCheckoutAsync(checkoutData);

            if (!result.Success)
            {
                return BadRequest(new CheckoutResponse
                {
                    Success = false,
                    ErrorMessage = result.ErrorMessage
                });
            }

            var orderDto = MapToOrderDto(result.Order!);

            return Ok(new CheckoutResponse
            {
                Success = true,
                Order = orderDto
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating order");
            return StatusCode(500, new CheckoutResponse
            {
                Success = false,
                ErrorMessage = "An error occurred while creating the order"
            });
        }
    }

    [HttpPut("{id}/status")]
    public async Task<ActionResult> UpdateOrderStatus(int id, [FromBody] UpdateOrderStatusRequest request)
    {
        try
        {
            var order = await _orderRepository.GetByIdAsync(id);
            if (order == null)
            {
                return NotFound();
            }

            order.Status = request.Status;

            switch (request.Status)
            {
                case OrderStatus.Shipped:
                    order.ShippedDate = DateTime.UtcNow;
                    break;
                case OrderStatus.Delivered:
                    order.DeliveredDate = DateTime.UtcNow;
                    break;
                case OrderStatus.Cancelled:
                    order.CancelledDate = DateTime.UtcNow;
                    break;
            }

            await _orderRepository.UpdateAsync(order);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating order status for {OrderId}", id);
            return StatusCode(500, "An error occurred while updating the order status");
        }
    }

    private string GenerateOrderNumber()
    {
        return $"ORD-{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid().ToString("N")[..6].ToUpperInvariant()}";
    }

    private Address MapToAddress(AddressDto dto)
    {
        return new Address
        {
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Street = dto.Street,
            City = dto.City,
            State = dto.State,
            PostalCode = dto.PostalCode,
            Country = dto.Country,
            Phone = dto.Phone,
            CreatedAt = DateTime.UtcNow
        };
    }

    private OrderDto MapToOrderDto(Order order)
    {
        return new OrderDto
        {
            Id = order.Id,
            OrderNumber = order.OrderNumber,
            UserId = order.UserId,
            Status = order.Status,
            PaymentStatus = order.PaymentStatus,
            SubTotal = order.SubTotal,
            DiscountAmount = order.DiscountAmount,
            TaxAmount = order.TaxAmount,
            ShippingAmount = order.ShippingAmount,
            TotalAmount = order.TotalAmount,
            CouponCode = order.CouponCode,
            TaxRate = order.TaxRate,
            PaymentMethod = order.PaymentMethod,
            PaymentTransactionId = order.PaymentTransactionId,
            OrderDate = order.OrderDate,
            ShippedDate = order.ShippedDate,
            DeliveredDate = order.DeliveredDate,
            CancelledDate = order.CancelledDate,
            ShippingAddress = order.ShippingAddress != null ? MapToAddressDto(order.ShippingAddress) : null,
            BillingAddress = order.BillingAddress != null ? MapToAddressDto(order.BillingAddress) : null,
            Items = order.Items.Select(item => new OrderItemDto
            {
                Id = item.Id,
                ProductId = item.ProductId,
                ProductName = item.Product?.Name ?? "Unknown Product",
                ProductSku = item.Product?.SKU,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice,
                TotalPrice = item.TotalPrice
            }).ToList()
        };
    }

    private AddressDto MapToAddressDto(Address address)
    {
        return new AddressDto
        {
            Id = address.Id,
            FirstName = address.FirstName,
            LastName = address.LastName,
            Street = address.Street,
            City = address.City,
            State = address.State,
            PostalCode = address.PostalCode,
            Country = address.Country,
            Phone = address.Phone
        };
    }
}

public record UpdateOrderStatusRequest(OrderStatus Status);


