using EcomShopping.Application.DTOs;
using EcomShopping.Domain.Entities;
using EcomShopping.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace EcomShopping.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CouponsController : ControllerBase
{
    private readonly ICouponRepository _couponRepository;
    private readonly ILogger<CouponsController> _logger;

    public CouponsController(
        ICouponRepository couponRepository,
        ILogger<CouponsController> logger)
    {
        _couponRepository = couponRepository;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<CouponDto>>> GetCoupons()
    {
        try
        {
            var coupons = await _couponRepository.GetAllAsync();
            var couponDtos = coupons.Select(MapToCouponDto).ToList();
            return Ok(couponDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving coupons");
            return StatusCode(500, "An error occurred while retrieving coupons");
        }
    }

    [HttpGet("active")]
    public async Task<ActionResult<IEnumerable<CouponDto>>> GetActiveCoupons()
    {
        try
        {
            var coupons = await _couponRepository.GetActiveAsync();
            var couponDtos = coupons.Select(MapToCouponDto).ToList();
            return Ok(couponDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving active coupons");
            return StatusCode(500, "An error occurred while retrieving active coupons");
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<CouponDto>> GetCoupon(int id)
    {
        try
        {
            var coupon = await _couponRepository.GetByIdAsync(id);
            if (coupon == null)
            {
                return NotFound();
            }

            return Ok(MapToCouponDto(coupon));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving coupon {CouponId}", id);
            return StatusCode(500, "An error occurred while retrieving the coupon");
        }
    }

    [HttpPost("validate")]
    public async Task<ActionResult<ValidateCouponResponse>> ValidateCoupon([FromBody] ValidateCouponRequest request)
    {
        try
        {
            var coupon = await _couponRepository.GetByCodeAsync(request.Code);
            
            if (coupon == null)
            {
                return Ok(new ValidateCouponResponse
                {
                    IsValid = false,
                    ErrorMessage = "Coupon code not found"
                });
            }

            var now = DateTime.UtcNow;
            
            // Check if active
            if (!coupon.IsActive)
            {
                return Ok(new ValidateCouponResponse
                {
                    IsValid = false,
                    ErrorMessage = "This coupon is no longer active"
                });
            }

            // Check validity dates
            if (coupon.ValidFrom.HasValue && coupon.ValidFrom.Value > now)
            {
                return Ok(new ValidateCouponResponse
                {
                    IsValid = false,
                    ErrorMessage = $"This coupon is not valid until {coupon.ValidFrom.Value:yyyy-MM-dd}"
                });
            }

            if (coupon.ValidUntil.HasValue && coupon.ValidUntil.Value < now)
            {
                return Ok(new ValidateCouponResponse
                {
                    IsValid = false,
                    ErrorMessage = "This coupon has expired"
                });
            }

            // Check usage limit
            if (coupon.UsageLimit.HasValue && coupon.UsageCount >= coupon.UsageLimit.Value)
            {
                return Ok(new ValidateCouponResponse
                {
                    IsValid = false,
                    ErrorMessage = "This coupon has reached its usage limit"
                });
            }

            // Check minimum order amount
            if (coupon.MinimumOrderAmount.HasValue && request.OrderAmount < coupon.MinimumOrderAmount.Value)
            {
                return Ok(new ValidateCouponResponse
                {
                    IsValid = false,
                    ErrorMessage = $"Minimum order amount of {coupon.MinimumOrderAmount.Value:C} required"
                });
            }

            // Calculate discount
            decimal discountAmount = 0;
            switch (coupon.Type)
            {
                case CouponType.Percentage:
                    discountAmount = Math.Round(request.OrderAmount * (coupon.Value / 100), 2);
                    break;
                case CouponType.FixedAmount:
                    discountAmount = coupon.Value;
                    break;
                case CouponType.FreeShipping:
                    discountAmount = 0; // Discount is applied to shipping
                    break;
            }

            // Apply maximum discount cap if set
            if (coupon.MaximumDiscountAmount.HasValue && discountAmount > coupon.MaximumDiscountAmount.Value)
            {
                discountAmount = coupon.MaximumDiscountAmount.Value;
            }

            return Ok(new ValidateCouponResponse
            {
                IsValid = true,
                Coupon = MapToCouponDto(coupon),
                DiscountAmount = discountAmount
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating coupon");
            return StatusCode(500, "An error occurred while validating the coupon");
        }
    }

    [HttpPost]
    public async Task<ActionResult<CouponDto>> CreateCoupon([FromBody] CreateCouponDto dto)
    {
        try
        {
            var coupon = new Coupon
            {
                Code = dto.Code,
                Description = dto.Description,
                Type = dto.Type,
                Value = dto.Value,
                MinimumOrderAmount = dto.MinimumOrderAmount,
                MaximumDiscountAmount = dto.MaximumDiscountAmount,
                ValidFrom = dto.ValidFrom,
                ValidUntil = dto.ValidUntil,
                UsageLimit = dto.UsageLimit,
                UsageCount = 0,
                IsActive = true
            };

            var createdCoupon = await _couponRepository.AddAsync(coupon);
            return CreatedAtAction(nameof(GetCoupon), new { id = createdCoupon.Id }, MapToCouponDto(createdCoupon));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating coupon");
            return StatusCode(500, "An error occurred while creating the coupon");
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateCoupon(int id, [FromBody] CreateCouponDto dto)
    {
        try
        {
            var coupon = await _couponRepository.GetByIdAsync(id);
            if (coupon == null)
            {
                return NotFound();
            }

            coupon.Code = dto.Code;
            coupon.Description = dto.Description;
            coupon.Type = dto.Type;
            coupon.Value = dto.Value;
            coupon.MinimumOrderAmount = dto.MinimumOrderAmount;
            coupon.MaximumDiscountAmount = dto.MaximumDiscountAmount;
            coupon.ValidFrom = dto.ValidFrom;
            coupon.ValidUntil = dto.ValidUntil;
            coupon.UsageLimit = dto.UsageLimit;

            await _couponRepository.UpdateAsync(coupon);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating coupon {CouponId}", id);
            return StatusCode(500, "An error occurred while updating the coupon");
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteCoupon(int id)
    {
        try
        {
            var coupon = await _couponRepository.GetByIdAsync(id);
            if (coupon == null)
            {
                return NotFound();
            }

            await _couponRepository.DeleteAsync(id);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting coupon {CouponId}", id);
            return StatusCode(500, "An error occurred while deleting the coupon");
        }
    }

    private CouponDto MapToCouponDto(Coupon coupon)
    {
        return new CouponDto
        {
            Id = coupon.Id,
            Code = coupon.Code,
            Description = coupon.Description,
            Type = coupon.Type,
            Value = coupon.Value,
            MinimumOrderAmount = coupon.MinimumOrderAmount,
            MaximumDiscountAmount = coupon.MaximumDiscountAmount,
            ValidFrom = coupon.ValidFrom,
            ValidUntil = coupon.ValidUntil,
            UsageLimit = coupon.UsageLimit,
            UsageCount = coupon.UsageCount,
            IsActive = coupon.IsActive
        };
    }
}
