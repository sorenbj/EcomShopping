using EcomShopping.Domain.Entities;
using EcomShopping.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace EcomShopping.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly IOrderRepository _orderRepository;
    private readonly ICartRepository _cartRepository;
    private readonly ILogger<OrdersController> _logger;

    public OrdersController(
        IOrderRepository orderRepository,
        ICartRepository cartRepository,
        ILogger<OrdersController> logger)
    {
        _orderRepository = orderRepository;
        _cartRepository = cartRepository;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Order>>> GetOrders([FromQuery] string? userId)
    {
        try
        {
            if (string.IsNullOrEmpty(userId))
            {
                var allOrders = await _orderRepository.GetAllAsync();
                return Ok(allOrders);
            }

            var orders = await _orderRepository.GetByUserIdAsync(userId);
            return Ok(orders);
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
            return Ok(order);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving order {OrderId}", id);
            return StatusCode(500, "An error occurred while retrieving the order");
        }
    }

    [HttpPost("checkout")]
    public async Task<ActionResult<Order>> Checkout([FromBody] CheckoutRequest request)
    {
        try
        {
            var cart = await _cartRepository.GetBySessionIdAsync(request.SessionId)
                ?? (request.UserId != null ? await _cartRepository.GetByUserIdAsync(request.UserId) : null);

            if (cart == null || !cart.Items.Any())
            {
                return BadRequest("Cart is empty");
            }

            var order = new Order
            {
                OrderNumber = GenerateOrderNumber(),
                UserId = request.UserId,
                Status = OrderStatus.Pending,
                OrderDate = DateTime.UtcNow,
                ShippingAddressId = request.ShippingAddressId,
                BillingAddressId = request.BillingAddressId,
                Items = cart.Items.Select(ci => new OrderItem
                {
                    ProductId = ci.ProductId,
                    Quantity = ci.Quantity,
                    UnitPrice = ci.UnitPrice,
                    TotalPrice = ci.Quantity * ci.UnitPrice
                }).ToList()
            };

            order.TotalAmount = order.Items.Sum(i => i.TotalPrice);

            var createdOrder = await _orderRepository.AddAsync(order);

            // Clear the cart after successful order creation
            await _cartRepository.DeleteAsync(cart.Id);

            return CreatedAtAction(nameof(GetOrder), new { id = createdOrder.Id }, createdOrder);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating order");
            return StatusCode(500, "An error occurred while creating the order");
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
        return $"ORD-{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid().ToString("N").Substring(0, 6).ToUpper()}";
    }
}

public record CheckoutRequest(string SessionId, string? UserId, int? ShippingAddressId, int? BillingAddressId);
public record UpdateOrderStatusRequest(OrderStatus Status);
