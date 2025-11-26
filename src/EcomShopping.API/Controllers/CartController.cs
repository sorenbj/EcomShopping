using EcomShopping.Domain.Entities;
using EcomShopping.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace EcomShopping.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CartController : ControllerBase
{
    private readonly ICartRepository _cartRepository;
    private readonly IProductRepository _productRepository;
    private readonly ILogger<CartController> _logger;

    public CartController(
        ICartRepository cartRepository,
        IProductRepository productRepository,
        ILogger<CartController> logger)
    {
        _cartRepository = cartRepository;
        _productRepository = productRepository;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<Cart>> GetCart([FromQuery] string? sessionId, [FromQuery] string? userId)
    {
        try
        {
            Cart? cart = null;

            if (!string.IsNullOrEmpty(userId))
            {
                cart = await _cartRepository.GetByUserIdAsync(userId);
            }
            else if (!string.IsNullOrEmpty(sessionId))
            {
                cart = await _cartRepository.GetBySessionIdAsync(sessionId);
            }

            if (cart == null)
            {
                return NotFound("Cart not found");
            }

            return Ok(cart);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving cart");
            return StatusCode(500, "An error occurred while retrieving the cart");
        }
    }

    [HttpPost]
    public async Task<ActionResult<Cart>> CreateCart([FromBody] Cart cart)
    {
        try
        {
            var createdCart = await _cartRepository.AddAsync(cart);
            return CreatedAtAction(nameof(GetCart), new { sessionId = createdCart.SessionId }, createdCart);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating cart");
            return StatusCode(500, "An error occurred while creating the cart");
        }
    }

    [HttpPost("items")]
    public async Task<ActionResult> AddCartItem([FromBody] CartItemRequest request)
    {
        try
        {
            var cart = await GetOrCreateCart(request.SessionId, request.UserId);
            if (cart == null)
            {
                return BadRequest("Could not create cart");
            }

            var product = await _productRepository.GetByIdAsync(request.ProductId);
            if (product == null)
            {
                return NotFound("Product not found");
            }

            var cartItem = new CartItem
            {
                CartId = cart.Id,
                ProductId = request.ProductId,
                Quantity = request.Quantity,
                UnitPrice = product.Price,
                AddedAt = DateTime.UtcNow
            };

            cart.Items.Add(cartItem);
            await _cartRepository.UpdateAsync(cart);

            return Ok(cart);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding item to cart");
            return StatusCode(500, "An error occurred while adding item to cart");
        }
    }

    [HttpPut("items/{id}")]
    public async Task<ActionResult> UpdateCartItem(int id, [FromBody] UpdateCartItemRequest request)
    {
        try
        {
            var cart = await _cartRepository.GetBySessionIdAsync(request.SessionId)
                ?? await _cartRepository.GetByUserIdAsync(request.UserId ?? "");

            if (cart == null)
            {
                return NotFound("Cart not found");
            }

            var cartItem = cart.Items.FirstOrDefault(i => i.Id == id);
            if (cartItem == null)
            {
                return NotFound("Cart item not found");
            }

            cartItem.Quantity = request.Quantity;
            await _cartRepository.UpdateAsync(cart);

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating cart item {CartItemId}", id);
            return StatusCode(500, "An error occurred while updating the cart item");
        }
    }

    [HttpDelete("items/{id}")]
    public async Task<ActionResult> RemoveCartItem(int id, [FromQuery] string sessionId, [FromQuery] string? userId)
    {
        try
        {
            var cart = await _cartRepository.GetBySessionIdAsync(sessionId)
                ?? await _cartRepository.GetByUserIdAsync(userId ?? "");

            if (cart == null)
            {
                return NotFound("Cart not found");
            }

            var cartItem = cart.Items.FirstOrDefault(i => i.Id == id);
            if (cartItem == null)
            {
                return NotFound("Cart item not found");
            }

            cart.Items.Remove(cartItem);
            await _cartRepository.UpdateAsync(cart);

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing cart item {CartItemId}", id);
            return StatusCode(500, "An error occurred while removing the cart item");
        }
    }

    private async Task<Cart?> GetOrCreateCart(string sessionId, string? userId)
    {
        var cart = await _cartRepository.GetBySessionIdAsync(sessionId)
            ?? (userId != null ? await _cartRepository.GetByUserIdAsync(userId) : null);

        if (cart == null)
        {
            cart = new Cart
            {
                SessionId = sessionId,
                UserId = userId,
                CreatedAt = DateTime.UtcNow
            };
            cart = await _cartRepository.AddAsync(cart);
        }

        return cart;
    }
}

public record CartItemRequest(string SessionId, string? UserId, int ProductId, int Quantity);
public record UpdateCartItemRequest(string SessionId, string? UserId, int Quantity);
