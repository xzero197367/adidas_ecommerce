using Adidas.Application.Contracts.ServicesContracts.Feature;
using Adidas.Application.Contracts.ServicesContracts.Operation;
using Adidas.DTOs.Feature.CouponDTOs;
using Adidas.DTOs.Operation.OrderDTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Adidas.ClientAPI.Controllers.Operation
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CheckoutController : ControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly ICouponService _couponService;

        public CheckoutController(IOrderService orderService, ICouponService couponService)
        {
            _orderService = orderService;
            _couponService = couponService;
        }

 
        [HttpGet("summary")]
        public async Task<IActionResult> GetOrderSummary([FromQuery] string? couponCode = null)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("User not authenticated.");

            var result = await _orderService.GetFormattedOrderSummaryAsync(userId, couponCode);
            if (!result.IsSuccess)
                return BadRequest(result.ErrorMessage);

            return Ok(result.Data);
        }

        
        [HttpGet("billing-summary")]
        public async Task<IActionResult> GetBillingSummary([FromQuery] string? promoCode = null)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("User not authenticated.");

            var summary = await _orderService.GetBillingSummaryAsync(userId, promoCode);
            return Ok(summary);
        }

     
        [HttpPost("apply-coupon")]
        public async Task<IActionResult> AddCouponToOrder([FromBody] ApplyCouponRequestDto dto)
        {
            var result = await _couponService.ApplyCouponToOrderAsync(dto.OrderId, dto.CouponCode);
            if (!result.Success)
                return BadRequest(result.Message);

            var response = new CouponAppliedDto
            {
                DiscountApplied = result.DiscountApplied,
                TotalAmount = result.NewTotal
            };

            return Ok(response);
        }
    }
}
