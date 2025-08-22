using Adidas.Application.Contracts.ServicesContracts.Feature;
using Adidas.Application.Contracts.ServicesContracts.Operation;
using Adidas.DTOs.Feature.CouponDTOs;
using Adidas.DTOs.Operation.OrderDTOs;
using Adidas.DTOs.Operation.PaymentDTOs;
using Adidas.DTOs.Operation.PaymentDTOs.PaypalDtos;
using Adidas.Models.Operation;
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
        private readonly IShoppingCartService _cartService;
        private readonly IPaymentService _paymentService;
        private readonly IPayPalService _payPalService;
        private readonly ILogger<CheckoutController> _logger;

        public CheckoutController(
            IOrderService orderService,
            ICouponService couponService,
            IShoppingCartService cartService,
            IPaymentService paymentService,
            IPayPalService payPalService,
            ILogger<CheckoutController> logger)
        {
            _orderService = orderService;
            _couponService = couponService;
            _cartService = cartService;
            _paymentService = paymentService;
            _payPalService = payPalService;
            _logger = logger;
        }

        /// <summary>
        /// Get checkout summary with cart items, addresses, and potential discounts
        /// </summary>
        [HttpGet("summary")]
        public async Task<IActionResult> GetCheckoutSummary([FromQuery] string? couponCode = null)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User not authenticated.");

                var result = await _orderService.GetFormattedOrderSummaryAsync(userId, couponCode);
                if (!result.IsSuccess)
                    return BadRequest(new { success = false, message = result.ErrorMessage });

                return Ok(new { success = true, data = result.Data });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting checkout summary for user {UserId}", User?.Identity?.Name);
                return StatusCode(500, new { success = false, message = "Internal server error" });
            }
        }

        /// <summary>
        /// Get billing summary for payment processing
        /// </summary>
        [HttpGet("billing-summary")]
        public async Task<IActionResult> GetBillingSummary([FromQuery] string? promoCode = null)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User not authenticated.");

                var summary = await _orderService.GetBillingSummaryAsync(userId, promoCode);
                return Ok(new { success = true, data = summary });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting billing summary for user {UserId}", User?.Identity?.Name);
                return StatusCode(500, new { success = false, message = "Internal server error" });
            }
        }

        /// <summary>
        /// Apply coupon to current cart/order
        /// </summary>
        [HttpPost("apply-coupon")]
        public async Task<IActionResult> ApplyCoupon([FromBody] ApplyCouponRequestDto dto)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User not authenticated.");

                var result = await _couponService.ApplyCouponToOrderAsync(dto.OrderId, dto.CouponCode);
                if (!result.Success)
                    return BadRequest(new { success = false, message = result.Message });

                var response = new CouponAppliedDto
                {
                    DiscountApplied = result.DiscountApplied,
                    TotalAmount = result.NewTotal
                };

                return Ok(new { success = true, data = response, message = "Coupon applied successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error applying coupon for user {UserId}", User?.Identity?.Name);
                return StatusCode(500, new { success = false, message = "Internal server error" });
            }
        }

        /// <summary>
        /// Process complete checkout - creates order and initiates payment
        /// </summary>
        [HttpPost("process")]
        public async Task<IActionResult> ProcessCheckout([FromBody] CheckoutRequestDto request)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User not authenticated.");

                _logger.LogInformation("Processing checkout for user {UserId}", userId);

                // Step 1: Validate cart has items
                var cartResult = await _cartService.GetCartItemsByUserIdAsync(userId);
                if (!cartResult.IsSuccess || !cartResult.Data.Any())
                    return BadRequest(new { success = false, message = "Cart is empty" });

                // Step 2: Create order from cart items
                var createOrderDto = new CreateOrderDTO
                {
                    UserId = userId,
                    ShippingAddress = request.ShippingAddress,
                    BillingAddress = request.BillingAddress,
                    CouponCode = request.CouponCode,
                    PaymentMethod = request.PaymentMethod,
                    Notes = request.Notes
                };

                var orderResult = await _orderService.CreateOrderFromCartAsync(createOrderDto);
                if (!orderResult.IsSuccess)
                    return BadRequest(new { success = false, message = orderResult.ErrorMessage });

                var orderId = orderResult.Data.Id;
                _logger.LogInformation("Order created with ID {OrderId} for user {UserId}", orderId, userId);

                // Step 3: Process payment based on method
                try
                {
                    if (request.PaymentMethod.ToLower() == "paypal")
                    {
                        return await ProcessPayPalPayment(orderResult.Data, request);
                    }
                    else
                    {
                        return await ProcessRegularPayment(orderResult.Data, request);
                    }
                }
                catch (Exception paymentEx)
                {
                    _logger.LogError(paymentEx, "Payment failed for order {OrderId}, deleting order", orderId);

                    // Payment failed - delete the order
                    await _orderService.DeleteAsync(orderId);

                    return BadRequest(new
                    {
                        success = false,
                        message = "Payment processing failed. Please try again.",
                        error = paymentEx.Message
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing checkout for user {UserId}", User?.Identity?.Name);
                return StatusCode(500, new { success = false, message = "Internal server error during checkout" });
            }
        }

        /// <summary>
        /// Complete checkout after successful payment
        /// </summary>
        [HttpPost("complete")]
        public async Task<IActionResult> CompleteCheckout([FromBody] CompleteCheckoutDto request)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User not authenticated.");

                _logger.LogInformation("Completing checkout for order {OrderId}", request.OrderId);

                // Verify payment was successful
                var paymentResult = await _paymentService.GetPaymentByIdAsync(request.PaymentId);
                if (!paymentResult.IsSuccess || paymentResult.Data.PaymentStatus != "Completed")
                    return BadRequest(new { success = false, message = "Payment not completed" });

                // Update order status to Processing
                var updateResult = await _orderService.UpdateOrderStatusAsync(request.OrderId, OrderStatus.Processing);
                if (!updateResult.IsSuccess)
                    return BadRequest(new { success = false, message = updateResult.ErrorMessage });

                // Clear user's cart
                var clearCartResult = await _cartService.ClearCartAsync(userId);
                if (!clearCartResult.IsSuccess)
                    _logger.LogWarning("Failed to clear cart for user {UserId} after successful checkout", userId);

                return Ok(new
                {
                    success = true,
                    message = "Checkout completed successfully",
                    orderId = request.OrderId,
                    paymentId = request.PaymentId
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error completing checkout");
                return StatusCode(500, new { success = false, message = "Internal server error" });
            }
        }

        private async Task<IActionResult> ProcessPayPalPayment(dynamic order, CheckoutRequestDto request)
        {
            var paypalDto = new PayPalCreatePaymentDto
            {
                Amount = order.TotalAmount,
                Currency = "USD",
                Description = $"Order #{order.Id}",
                OrderId = order.Id,
                ReturnUrl = $"{GetFrontendUrl()}/checkout/success",
                CancelUrl = $"{GetFrontendUrl()}/checkout/cancel"
            };

            var paymentResult = await _payPalService.CreatePaymentAsync(paypalDto);
            if (!paymentResult.IsSuccess)
            {
                throw new Exception($"PayPal payment creation failed: {paymentResult.ErrorMessage}");
            }

            return Ok(new
            {
                success = true,
                paymentMethod = "paypal",
                orderId = order.Id,
                paymentId = paymentResult.Data.PaymentId,
                approvalUrl = paymentResult.Data.ApprovalUrl,
                message = "PayPal payment created. Redirect user to approval URL."
            });
        }


        private async Task<IActionResult> ProcessRegularPayment(dynamic order, CheckoutRequestDto request)
        {
            var paymentDto = new PaymentCreateDto
            {
                OrderId = order.Id,
                Amount = order.TotalAmount,
                PaymentMethod = request.PaymentMethod
            };

            var paymentResult = await _paymentService.ProcessPaymentAsync(paymentDto);
            if (!paymentResult.IsSuccess)
            {
                throw new Exception($"Payment processing failed: {paymentResult.ErrorMessage}");
            }

            // If payment successful, complete the checkout
            var completeDto = new CompleteCheckoutDto
            {
                OrderId = order.Id,
                PaymentId = paymentResult.Data.Id
            };

            return await CompleteCheckout(completeDto);
        }

        private string GetFrontendUrl()
        {
            return Environment.GetEnvironmentVariable("FRONTEND_URL") ?? "http://localhost:4200";
        }
    }

    // DTOs
    public class CheckoutRequestDto
    {
        public string ShippingAddress { get; set; }
        public string BillingAddress { get; set; }
        public string? CouponCode { get; set; }
        public string PaymentMethod { get; set; } = "card"; // card, paypal
        public string? Notes { get; set; }
        public CardDetailsDto? CardDetails { get; set; }
    }

    public class CardDetailsDto
    {
        public string CardNumber { get; set; }
        public int ExpiryMonth { get; set; }
        public int ExpiryYear { get; set; }
        public string CVV { get; set; }
        public string CardholderName { get; set; }
    }

    public class CompleteCheckoutDto
    {
        public Guid OrderId { get; set; }
        public Guid PaymentId { get; set; }
    }


}