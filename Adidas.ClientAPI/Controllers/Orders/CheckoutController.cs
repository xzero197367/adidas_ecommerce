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

                // More concise null checking using null-conditional operator
                if (!paymentResult.IsSuccess || paymentResult.Data == null || paymentResult.Data.PaymentStatus != "Completed")
                    return BadRequest(new { success = false, message = "Payment not completed or not found" });

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

        /// <summary>
        /// Process regular payment (Cash on Delivery only)
        /// </summary>
        /// <summary>
        /// Process regular payment (Cash on Delivery only)
        /// </summary>
        private async Task<IActionResult> ProcessRegularPayment(dynamic order, CheckoutRequestDto request)
        {
            try
            {
                // Validate payment method - only allow Cash on Delivery
                if (!string.Equals(request.PaymentMethod, "Cash on Delivery", StringComparison.OrdinalIgnoreCase) &&
                    !string.Equals(request.PaymentMethod, "COD", StringComparison.OrdinalIgnoreCase))
                {
                    throw new Exception($"Invalid payment method: {request.PaymentMethod}. Only 'Cash on Delivery' is supported for regular payments.");
                }

                // Create payment DTO for Cash on Delivery
                var paymentDto = new PaymentCreateDto
                {
                    OrderId = (Guid)order.Id,
                    Amount = (decimal)order.TotalAmount,
                    PaymentMethod = "Cash on Delivery"
                };

                ((ILogger)_logger).LogInformation("Processing Cash on Delivery for Order {OrderId}, Amount {Amount}",
                    paymentDto.OrderId, paymentDto.Amount);

                var paymentResult = await _paymentService.ProcessPaymentAsync(paymentDto);
                if (!paymentResult.IsSuccess)
                {
                    throw new Exception($"Cash on Delivery processing failed: {paymentResult.ErrorMessage}");
                }

                // Check if payment data is null
                if (paymentResult.Data == null)
                {
                    throw new Exception("Payment service returned success but with null payment data");
                }

                // For COD, update order status to Confirmed (not Processing since payment isn't collected yet)
                var updateResult = await _orderService.UpdateOrderStatusAsync((Guid)order.Id, OrderStatus.Shipped);
                if (!updateResult.IsSuccess)
                {
                    ((ILogger)_logger).LogWarning("Failed to update order status for COD Order {OrderId}: {Message}");
                }

                // Clear user's cart since order is confirmed
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (!string.IsNullOrEmpty(userId))
                {
                    var clearCartResult = await _cartService.ClearCartAsync(userId);
                    if (!clearCartResult.IsSuccess)
                    {
                        ((ILogger)_logger).LogWarning("Failed to clear cart for user {UserId} after COD order confirmation", userId);
                    }
                }

                return Ok(new
                {
                    success = true,
                    paymentMethod = "Cash on Delivery",
                    orderId = order.Id,
                    paymentId = paymentResult.Data.Id,
                    orderStatus = "Confirmed",
                    message = "Cash on Delivery order confirmed successfully. Payment will be collected upon delivery."
                });
            }
            catch (Exception ex)
            {
                ((ILogger)_logger).LogError(ex, "Error processing Cash on Delivery payment for order {OrderId}");
                throw; // Re-throw to be handled by the calling method
            }
        }

        /// <summary>
        /// Process PayPal payment
        /// </summary>
        private async Task<IActionResult> ProcessPayPalPayment(dynamic order, CheckoutRequestDto request)
        {
            try
            {
                var paypalDto = new PayPalCreatePaymentDto
                {
                    Amount = (decimal)order.TotalAmount,
                    Currency = "USD",
                    Description = $"Order #{order.Id}",
                    OrderId = (Guid)order.Id,
                    ReturnUrl = $"{GetFrontendUrl()}/checkout/paypal/success",
                    CancelUrl = $"{GetFrontendUrl()}/checkout/paypal/cancel"
                };

                ((ILogger)_logger).LogInformation("Creating PayPal payment for Order {OrderId}, Amount {Amount}");

                var paymentResult = await _payPalService.CreatePaymentAsync(paypalDto);
                if (!paymentResult.IsSuccess)
                {
                    throw new Exception($"PayPal payment creation failed: {paymentResult.ErrorMessage}");
                }

                return Ok(new
                {
                    success = true,
                    paymentMethod = "PayPal",
                    orderId = order.Id,
                    paymentId = paymentResult.Data.PaymentId,
                    approvalUrl = paymentResult.Data.ApprovalUrl,
                    message = "PayPal payment created successfully. Redirect user to approval URL."
                });
            }
            catch (Exception ex)
            {
                ((ILogger)_logger).LogError(ex, "Error creating PayPal payment for order {OrderId}");
                throw; // Re-throw to be handled by the calling method
            }
        }

        /// <summary>
        /// Process PayPal payment
        /// </summary>
     

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