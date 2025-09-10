using Adidas.Application.Contracts.ServicesContracts.Feature;
using Adidas.Application.Contracts.ServicesContracts.Operation;
using Adidas.Application.Services.Static;
using Adidas.DTOs.Feature.CouponDTOs;
using Adidas.DTOs.Main.Product_DTOs;
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
    // Removed [Authorize] from class level to allow guest access
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
        /// Supports both authenticated users and guests
        /// </summary>
        [HttpPost("summary")] // Changed to POST to receive cart items from guests
        public async Task<IActionResult> GetCheckoutSummary([FromBody] CheckoutSummaryRequestDto request)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                bool isGuestUser = string.IsNullOrEmpty(userId);

                if (isGuestUser)
                {
                    // Guest user - validate required data
                    if (request.CartItems == null || !request.CartItems.Any())
                        return BadRequest(new { success = false, message = "Cart items are required for guest checkout" });

                    if (string.IsNullOrEmpty(request.GuestEmail))
                        return BadRequest(new { success = false, message = "Guest email is required" });

                    // Generate or use existing guest user ID
                    userId = string.IsNullOrEmpty(request.GuestUserId)
                        ? $"guest_{Guid.NewGuid()}"
                        : request.GuestUserId;

                    // Get summary for guest with provided cart items
                    var guestResult = await _orderService.GetGuestCheckoutSummaryAsync(userId, request.CartItems, request.CouponCode);
                    if (!guestResult.IsSuccess)
                        return BadRequest(new { success = false, message = guestResult.ErrorMessage });

                    return Ok(new
                    {
                        success = true,
                        data = guestResult.Data,
                        guestUserId = userId // Return guest ID for future use
                    });
                }
                else
                {
                    // Authenticated user
                    var result = await _orderService.GetFormattedOrderSummaryAsync(userId, request.CouponCode);
                    if (!result.IsSuccess)
                        return BadRequest(new { success = false, message = result.ErrorMessage });

                    return Ok(new { success = true, data = result.Data });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting checkout summary");
                return StatusCode(500, new { success = false, message = "Internal server error" });
            }
        }

        /// <summary>
        /// Get billing summary for payment processing
        /// Supports both authenticated users and guests
        /// </summary>
        [HttpPost("billing-summary")] // Changed to POST for guest support
        public async Task<IActionResult> GetBillingSummary([FromBody] BillingSummaryRequestDto request)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                bool isGuestUser = string.IsNullOrEmpty(userId);

                if (isGuestUser)
                {
                    if (request.CartItems == null || !request.CartItems.Any())
                        return BadRequest(new { success = false, message = "Cart items are required for guest billing summary" });

                    userId = string.IsNullOrEmpty(request.GuestUserId)
                        ? $"guest_{Guid.NewGuid()}"
                        : request.GuestUserId;

                    var guestSummary = await _orderService.GetGuestBillingSummaryAsync(userId, request.CartItems, request.PromoCode);
                    return Ok(new
                    {
                        success = true,
                        data = guestSummary,
                        guestUserId = userId
                    });
                }
                else
                {
                    var summary = await _orderService.GetBillingSummaryAsync(userId, request.PromoCode);
                    return Ok(new { success = true, data = summary });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting billing summary");
                return StatusCode(500, new { success = false, message = "Internal server error" });
            }
        }

        /// <summary>
        /// Apply coupon to current cart/order
        /// For authenticated users only (guests handle coupons in checkout summary)
        /// </summary>
        [HttpPost("apply-coupon")]
      //  [Authorize] // Authenticated users only
        public async Task<IActionResult> ApplyCoupon([FromBody] ApplyCouponRequestDto dto)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                //if (string.IsNullOrEmpty(userId))
                //    return Unauthorized("User not authenticated.");

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
        /// Supports both authenticated users and guests
        /// </summary>
        [HttpPost("process")]
        public async Task<IActionResult> ProcessCheckout([FromBody] CheckoutRequestDto request)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                bool isGuestUser = string.IsNullOrEmpty(userId);

                if (isGuestUser)
                {
                    return await ProcessGuestCheckout(request);
                }
                else
                {
                    return await ProcessAuthenticatedUserCheckout(userId, request);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing checkout");
                return StatusCode(500, new { success = false, message = "Internal server error during checkout" });
            }
        }

        /// <summary>
        /// Process checkout for authenticated users
        /// </summary>
        private async Task<IActionResult> ProcessAuthenticatedUserCheckout(string userId, CheckoutRequestDto request)
        {
            _logger.LogInformation("Processing checkout for authenticated user {UserId}", userId);

            // Step 1: Validate cart has items
            var cartResult = await _cartService.GetCartItemsByUserIdAsync(userId);
            if (!cartResult.IsSuccess || !cartResult.Data.Any())
                return BadRequest(new { success = false, message = "Cart is empty" });

            // Step 2: Create order from cart items
            var createOrderDto = new CreateOrderDTO
            {
                UserId = userId,
                IsGuestUser = false,
                ShippingAddress = request.ShippingAddress,
                BillingAddress = request.BillingAddress,
                Currency = request.Currency ?? "USD",
                CouponCode = request.CouponCode,
                PaymentMethod = request.PaymentMethod,
                Notes = request.Notes,
                CartItems = cartResult.Data.ToList()
            };

            return await CreateOrderAndProcessPayment(createOrderDto, request);
        }

        /// <summary>
        /// Process checkout for guest users
        /// </summary>
        private async Task<IActionResult> ProcessGuestCheckout(CheckoutRequestDto request)
        {
            // Validate guest checkout requirements
            if (request.CartItems == null || !request.CartItems.Any())
                return BadRequest(new { success = false, message = "Cart items are required for guest checkout" });

            if (string.IsNullOrEmpty(request.GuestEmail))
                return BadRequest(new { success = false, message = "Guest email is required" });

            // Generate or use existing guest user ID
            var userId = string.IsNullOrEmpty(request.GuestUserId)
                ? $"guest_{Guid.NewGuid()}"
                : request.GuestUserId;

            _logger.LogInformation("Processing checkout for guest user {UserId}", userId);

            // Convert guest cart items to ShoppingCartDto format
            var cartItems = ConvertGuestCartItemsToShoppingCartDto(request.CartItems, userId);

            // Create order DTO for guest
            var createOrderDto = new CreateOrderDTO
            {
                UserId = userId,
                IsGuestUser = true,
                GuestEmail = request.GuestEmail,
                ShippingAddress = request.ShippingAddress,
                BillingAddress = request.BillingAddress,
                Currency = request.Currency ?? "USD",
                CouponCode = request.CouponCode,
                PaymentMethod = request.PaymentMethod,
                Notes = request.Notes,
                CartItems = cartItems
            };

            return await CreateOrderAndProcessPayment(createOrderDto, request, userId);
        }

        /// <summary>
        /// Enhanced method to create order and process payment with better error handling
        /// </summary>
        private async Task<IActionResult> CreateOrderAndProcessPayment(CreateOrderDTO createOrderDto, CheckoutRequestDto request, string? guestUserId = null)
        {
            Guid? orderId = null;

            try
            {
                // Create order
                var orderResult = await _orderService.CreateOrderFromCartAsync(createOrderDto);
                if (!orderResult.IsSuccess)
                {
                    _logger.LogError("Failed to create order for user {UserId}: {ErrorMessage}",
                        createOrderDto.UserId, orderResult.ErrorMessage);
                    return BadRequest(new { success = false, message = orderResult.ErrorMessage });
                }

                orderId = orderResult.Data.Id;
                _logger.LogInformation("Order created with ID {OrderId} for user {UserId}", orderId, createOrderDto.UserId);

                // Process payment based on method
                var paymentMethod = request.PaymentMethod?.ToLower();

                if (paymentMethod == "paypal")
                {
                    var paypalResult = await ProcessPayPalPayment(orderResult.Data, request);

                    if (guestUserId != null && paypalResult is OkObjectResult okResult)
                    {
                        // Extract the response data
                        var responseValue = okResult.Value;
                        var responseType = responseValue.GetType();

                        // Get property values using reflection for dynamic response
                        var successProp = responseType.GetProperty("success")?.GetValue(responseValue);
                        var paymentIdProp = responseType.GetProperty("paymentId")?.GetValue(responseValue);
                        var approvalUrlProp = responseType.GetProperty("approvalUrl")?.GetValue(responseValue);
                        var messageProp = responseType.GetProperty("message")?.GetValue(responseValue);

                        return Ok(new
                        {
                            success = successProp ?? true,
                            paymentMethod = "PayPal",
                            orderId = orderResult.Data.Id,
                            paymentId = paymentIdProp,
                            approvalUrl = approvalUrlProp,
                            approval_url = approvalUrlProp, // Alternative naming
                            redirectUrl = approvalUrlProp,  // Alternative naming
                            guestUserId = guestUserId,
                            message = messageProp ?? "PayPal payment created successfully. Redirect user to approval URL."
                        });
                    }
                    return paypalResult;
                }
                else if (paymentMethod == "cash on delivery" || paymentMethod == "cod")
                {
                    var regularResult = await ProcessRegularPayment(orderResult.Data, request);

                    if (guestUserId != null && regularResult is OkObjectResult okResult)
                    {
                        // Extract the response data
                        var responseValue = okResult.Value;
                        var responseType = responseValue.GetType();

                        var successProp = responseType.GetProperty("success")?.GetValue(responseValue);
                        var paymentIdProp = responseType.GetProperty("paymentId")?.GetValue(responseValue);
                        var messageProp = responseType.GetProperty("message")?.GetValue(responseValue);

                        return Ok(new
                        {
                            success = successProp ?? true,
                            paymentMethod = "Cash on Delivery",
                            orderId = orderResult.Data.Id,
                            paymentId = paymentIdProp,
                            orderStatus = "Confirmed",
                            guestUserId = guestUserId,
                            message = messageProp ?? "Cash on Delivery order confirmed successfully."
                        });
                    }
                    return regularResult;
                }
                else
                {
                    throw new Exception($"Unsupported payment method: {request.PaymentMethod}");
                }
            }
            catch (Exception paymentEx)
            {
                _logger.LogError(paymentEx, "Payment failed for order {OrderId}, attempting to delete order", orderId);

                // Payment failed - attempt to delete the order if it was created
                if (orderId.HasValue)
                {
                    try
                    {
                        await _orderService.DeleteAsync(orderId.Value);
                        _logger.LogInformation("Successfully deleted failed order {OrderId}", orderId.Value);
                    }
                    catch (Exception deleteEx)
                    {
                        _logger.LogError(deleteEx, "Failed to delete order {OrderId} after payment failure", orderId.Value);
                    }
                }

                return BadRequest(new
                {
                    success = false,
                    message = "Payment processing failed. Please try again.",
                    error = paymentEx.Message,
                    orderId = orderId // Include order ID for debugging
                });
            }
        }

        /// <summary>
        /// Complete checkout after successful payment
        /// Supports both authenticated users and guests
        /// </summary>
        [HttpPost("complete")]
        public async Task<IActionResult> CompleteCheckout([FromBody] CompleteCheckoutDto request)
        {
            try
            {
                _logger.LogInformation("Completing checkout for order {OrderId}", request.OrderId);

                // Get order details to check if it's a guest order
                var orderResult = await _orderService.GetOrderByIdAsync(request.OrderId);
                if (!orderResult.IsSuccess)
                    return BadRequest(new { success = false, message = "Order not found" });

                var isGuestOrder = orderResult.Data.UserId.StartsWith("guest_");
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                // For authenticated users, verify ownership
                if (!isGuestOrder && (string.IsNullOrEmpty(userId) || orderResult.Data.UserId != userId))
                    return Unauthorized("Access denied to this order");

                // For guest orders, allow completion without authentication (PayPal handles verification)
                if (isGuestOrder && string.IsNullOrEmpty(request.GuestEmail))
                    return BadRequest(new { success = false, message = "Guest email is required for guest orders" });

                // Verify payment was successful
                var paymentResult = await _paymentService.GetPaymentByIdAsync(request.PaymentId);
                if (!paymentResult.IsSuccess || paymentResult.Data == null || paymentResult.Data.PaymentStatus != "Completed")
                    return BadRequest(new { success = false, message = "Payment not completed or not found" });

                // Update order status to Processing
                var updateResult = await _orderService.UpdateOrderStatusAsync(request.OrderId, OrderStatus.Processing);
                if (!updateResult.IsSuccess)
                    return BadRequest(new { success = false, message = updateResult.ErrorMessage });

                // Clear cart only for authenticated users (guests don't have persistent carts)
                if (!isGuestOrder && !string.IsNullOrEmpty(userId))
                {
                    var clearCartResult = await _cartService.ClearCartAsync(userId);
                    if (!clearCartResult.IsSuccess)
                        _logger.LogWarning("Failed to clear cart for user {UserId} after successful checkout", userId);
                }

                return Ok(new
                {
                    success = true,
                    message = "Checkout completed successfully",
                    orderId = request.OrderId,
                    paymentId = request.PaymentId,
                    isGuestOrder = isGuestOrder
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error completing checkout");
                return StatusCode(500, new { success = false, message = "Internal server error" });
            }
        }

        /// <summary>
        /// Convert guest cart items to ShoppingCartDto format
        /// </summary>
        private List<Adidas.DTOs.Feature.ShoppingCartDTOS.ShoppingCartDto> ConvertGuestCartItemsToShoppingCartDto(List<GuestCartItemsDto> guestItems, string userId)
        {
            return guestItems.Select(item => new Adidas.DTOs.Feature.ShoppingCartDTOS.ShoppingCartDto
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                VariantId = item.VariantId,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice,
                SalePrice = item.UnitPrice,
                Variant = new DTOs.Main.Product_Variant_DTOs.ProductVariantDto
                {
                    Id = item.VariantId,
                    Sku = item.Sku,
                    StockQuantity = item.MaxStock,
                    Size = item.Size,
                    Color = item.Color,
                    Product = new ProductDto
                    {
                        Id = item.ProductId,
                        Name = item.ProductName,
                        Price = item.OriginalPrice,
                        SalePrice = item.UnitPrice
                    }
                }
            }).ToList();
        }

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

                _logger.LogInformation("Processing Cash on Delivery for Order {OrderId}, Amount {Amount}",
                    paymentDto.OrderId, paymentDto.Amount);

                var paymentResult = await _paymentService.ProcessPaymentAsync(paymentDto);
                if (!paymentResult.IsSuccess)
                {
                    throw new Exception($"Cash on Delivery processing failed: {paymentResult.ErrorMessage}");
                }

                if (paymentResult.Data == null)
                {
                    throw new Exception("Payment service returned success but with null payment data");
                }

                // For COD, update order status to Shipped
                var updateResult = await _orderService.UpdateOrderStatusAsync((Guid)order.Id, OrderStatus.Pending);
                if (!updateResult.IsSuccess)
                {
                    _logger.LogWarning("Failed to update order status for COD Order {OrderId}: {Message}"
                        );
                }

                // Clear authenticated user's cart
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (!string.IsNullOrEmpty(userId))
                {
                    var clearCartResult = await _cartService.ClearCartAsync(userId);
                    if (!clearCartResult.IsSuccess)
                    {
                        _logger.LogWarning("Failed to clear cart for user {UserId} after COD order confirmation", userId);
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
                _logger.LogError(ex, "Error processing Cash on Delivery payment for order {OrderId}");
                throw;
            }
        }

        /// <summary>
        /// Fixed ProcessPayPalPayment method with enhanced error handling and logging
        /// </summary>
        private async Task<IActionResult> ProcessPayPalPayment(dynamic order, CheckoutRequestDto request)
        {
            var egpAmount = (decimal)order.TotalAmount;

            // نحول من EGP → USD
            var usdAmount = CurrencyConverter.ConvertEgpToUsd(egpAmount);
            try
            {
                var paypalDto = new PayPalCreatePaymentDto
                {
                    Amount = usdAmount,
                    Currency = "USD", // Use request currency or default to USD
                    Description = $"Order #{order.Id}",
                    OrderId = (Guid)order.Id,
                    ReturnUrl = $"{GetFrontendUrl()}/checkout/paypal/success",
                    CancelUrl = $"{GetFrontendUrl()}/checkout/paypal/cancel"
                };

                _logger.LogInformation("Creating PayPal payment for Order {OrderId}, Amount {Amount}, Currency {Currency}",
                    paypalDto.OrderId, paypalDto.Amount, paypalDto.Currency);

                var paymentResult = await _payPalService.CreatePaymentAsync(paypalDto);

                // Enhanced error checking and logging
                if (!paymentResult.IsSuccess)
                {
                    _logger.LogError("PayPal payment creation failed for Order {OrderId}: {ErrorMessage}",
                        paypalDto.OrderId, paymentResult.ErrorMessage);
                    throw new Exception($"PayPal payment creation failed: {paymentResult.ErrorMessage}");
                }

                if (paymentResult.Data == null)
                {
                    _logger.LogError("PayPal service returned success but with null data for Order {OrderId}", paypalDto.OrderId);
                    throw new Exception("PayPal service returned success but with null payment data");
                }

                // Log the PayPal response structure for debugging
                _logger.LogInformation("PayPal payment created successfully for Order {OrderId}. PaymentId: {PaymentId}, ApprovalUrl: {ApprovalUrl}",
                    paypalDto.OrderId, paymentResult.Data.PaymentId, paymentResult.Data.ApprovalUrl);

                // Validate that ApprovalUrl is present and valid
                if (string.IsNullOrEmpty(paymentResult.Data.ApprovalUrl))
                {
                    _logger.LogError("PayPal payment created but ApprovalUrl is missing for Order {OrderId}", paypalDto.OrderId);
                    throw new Exception("PayPal payment created but approval URL is missing");
                }

                // Validate ApprovalUrl format
                if (!Uri.TryCreate(paymentResult.Data.ApprovalUrl, UriKind.Absolute, out var approvalUri) ||
                    (approvalUri.Scheme != "http" && approvalUri.Scheme != "https"))
                {
                    _logger.LogError("PayPal returned invalid ApprovalUrl for Order {OrderId}: {ApprovalUrl}",
                        paypalDto.OrderId, paymentResult.Data.ApprovalUrl);
                    throw new Exception("PayPal returned an invalid approval URL");
                }

                return Ok(new
                {
                    success = true,
                    paymentMethod = "PayPal",
                    orderId = order.Id,
                    paymentId = paymentResult.Data.PaymentId,
                    approvalUrl = paymentResult.Data.ApprovalUrl,
                    // Alternative property names for frontend compatibility
                    approval_url = paymentResult.Data.ApprovalUrl,
                    redirectUrl = paymentResult.Data.ApprovalUrl,
                    paypalUrl = paymentResult.Data.ApprovalUrl,
                    message = "PayPal payment created successfully. Redirect user to approval URL."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating PayPal payment for order {OrderId}: {Message}");
                throw;
            }
        }


        /// <summary>
        /// Enhanced GetFrontendUrl with environment-specific configuration
        /// </summary>
        private string GetFrontendUrl()
        {
            var frontendUrl = Environment.GetEnvironmentVariable("FRONTEND_URL");

            if (string.IsNullOrEmpty(frontendUrl))
            {
                // Default URLs based on environment
                var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";

                frontendUrl = environment.ToLower() switch
                {
                    "development" => "http://localhost:4200",
                    "staging" => "https://staging.yourdomain.com",
                    "production" => "https://yourdomain.com",
                    _ => "http://localhost:4200"
                };

                _logger.LogWarning("FRONTEND_URL environment variable not set, using default: {FrontendUrl}", frontendUrl);
            }

            return frontendUrl;
        }

    }

    // Updated DTOs
    public class CheckoutRequestDto
    {
        public string ShippingAddress { get; set; }
        public string BillingAddress { get; set; }
        public string? Currency { get; set; } = "USD";
        public string? CouponCode { get; set; }
        public string PaymentMethod { get; set; } = "Cash on Delivery";
        public string? Notes { get; set; }

        // Guest-specific properties
        public string? GuestUserId { get; set; }
        public string? GuestEmail { get; set; }
        public List<GuestCartItemsDto>? CartItems { get; set; }
    }

    public class CheckoutSummaryRequestDto
    {
        public string? CouponCode { get; set; }

        // Guest-specific properties
        public string? GuestUserId { get; set; }
        public string? GuestEmail { get; set; }
        public List<GuestCartItemsDto>? CartItems { get; set; }
    }

    public class BillingSummaryRequestDto
    {
        public string? PromoCode { get; set; }

        // Guest-specific properties
        public string? GuestUserId { get; set; }
        public List<GuestCartItemsDto>? CartItems { get; set; }
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
        public string? GuestEmail { get; set; } // Required for guest orders
    }
}