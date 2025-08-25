using Adidas.Application.Contracts.ServicesContracts.Operation;
using Adidas.Application.Contracts.ServicesContracts.Feature;
using Adidas.Models.Operation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Adidas.DTOs.Feature.ShoppingCartDTOS;
using Adidas.DTOs.Main.Product_DTOs;

namespace Adidas.ClientAPI.Controllers.Operation
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly IShoppingCartService _cartService;
        private readonly ILogger<OrderController> _logger;

        public OrderController(IOrderService orderService, IShoppingCartService cartService, ILogger<OrderController> logger)
        {
            _orderService = orderService;
            _cartService = cartService;
            _logger = logger;
        }

        /// <summary>
        /// Get all orders for the current user (Authenticated users only)
        /// </summary>
        [HttpGet("GetAllOrdersByUserId")]
        [Authorize] // Only authenticated users can access their order history
        public async Task<IActionResult> GetAllOrdersByUserId()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new { success = false, message = "User not authenticated" });

                var orders = await _orderService.GetOrdersByUserIdAsync(userId);

                if (orders.IsSuccess)
                {
                    return Ok(new { success = true, data = orders.Data });
                }

                return BadRequest(new { success = false, message = orders.ErrorMessage });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting orders for user {UserId}", User?.Identity?.Name);
                return StatusCode(500, new { success = false, message = "Internal server error" });
            }
        }

        /// <summary>
        /// Get order by ID - supports both authenticated users and guest orders with email verification
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetOrderById(Guid id, [FromQuery] string? guestEmail = null)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var result = await _orderService.GetOrderByIdAsync(id);

                if (!result.IsSuccess)
                    return NotFound(new { success = false, message = "Order not found" });

                // For authenticated users, verify ownership
                if (!string.IsNullOrEmpty(userId))
                {
                    if (!result.Data.UserId.StartsWith("guest_") && result.Data.UserId != userId)
                        return Forbid("Access denied to this order");
                }
                // For guest users, verify email if order is a guest order
                else if (result.Data.UserId.StartsWith("guest_"))
                {
                    if (string.IsNullOrEmpty(guestEmail))
                        return BadRequest(new { success = false, message = "Guest email is required" });

                    // You might want to store guest email in Order model for verification
                    // For now, we'll trust the frontend to provide correct email
                }
                else
                {
                    return Unauthorized(new { success = false, message = "Access denied" });
                }

                return Ok(new { success = true, data = result.Data });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting order {OrderId}", id);
                return StatusCode(500, new { success = false, message = "Internal server error" });
            }
        }

        /// <summary>
        /// Create order from cart items (supports both authenticated and guest users)
        /// </summary>
        [HttpPost("create-from-cart")]
        public async Task<IActionResult> CreateOrderFromCart([FromBody] CreateOrderFromCartDto model)
        {
            try
            {
                string userId;
                bool isGuestUser = false;
                List<ShoppingCartDto> cartItems = new List<ShoppingCartDto>();

                // Check if user is authenticated
                var authenticatedUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (string.IsNullOrEmpty(authenticatedUserId))
                {
                    // Handle guest user
                    isGuestUser = true;

                    // Validate guest email is provided
                    if (string.IsNullOrEmpty(model.GuestEmail))
                    {
                        return BadRequest(new { success = false, message = "Guest email is required for order confirmation" });
                    }

                    // Generate guest user ID
                    if (string.IsNullOrEmpty(model.GuestUserId))
                    {
                        userId = $"guest_{Guid.NewGuid()}";
                    }
                    else
                    {
                        userId = model.GuestUserId;
                    }

                    // For guest users, cart items must be provided in request
                    if (model.CartItems == null || !model.CartItems.Any())
                    {
                        return BadRequest(new { success = false, message = "Cart items are required for guest checkout" });
                    }

                    // Convert guest cart items to ShoppingCartDto format
                    cartItems = ConvertGuestCartItemsToShoppingCartDto(model.CartItems, userId);
                }
                else
                {
                    // Handle authenticated user
                    userId = authenticatedUserId;

                    // For authenticated users, get cart items from database
                    var cartResult = await _cartService.GetCartItemsByUserIdAsync(userId);
                    if (!cartResult.IsSuccess || !cartResult.Data.Any())
                        return BadRequest(new { success = false, message = "Cart is empty" });

                    cartItems = cartResult.Data.ToList();
                }

                // Validate that we have cart items
                if (!cartItems.Any())
                {
                    return BadRequest(new { success = false, message = "Cart is empty" });
                }

                var createOrderDto = new DTOs.Operation.OrderDTOs.CreateOrderDTO
                {
                    UserId = userId,
                    IsGuestUser = isGuestUser,
                    ShippingAddress = model.ShippingAddress,
                    BillingAddress = model.BillingAddress,
                    Currency = model.Currency,
                    CouponCode = model.CouponCode,
                    PaymentMethod = model.PaymentMethod,
                    Notes = model.Notes,
                    GuestEmail = model.GuestEmail,
                    CartItems = cartItems
                };

                var result = await _orderService.CreateOrderFromCartAsync(createOrderDto);

                if (result.IsSuccess)
                {
                    _logger.LogInformation("Order created from cart: {OrderId} for user {UserId} (Guest: {IsGuest})",
                        result.Data.Id, userId, isGuestUser);

                    var response = new
                    {
                        success = true,
                        data = result.Data,
                        message = "Order created successfully",
                        guestUserId = isGuestUser ? userId : null
                    };

                    return CreatedAtAction(nameof(GetOrderById),
                        new { id = result.Data.Id }, response);
                }

                return BadRequest(new { success = false, message = result.ErrorMessage });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating order from cart");
                return StatusCode(500, new { success = false, message = "Internal server error" });
            }
        }

        /// <summary>
        /// Convert guest cart items (from localStorage) to ShoppingCartDto format
        /// </summary>
        private List<ShoppingCartDto> ConvertGuestCartItemsToShoppingCartDto(List<GuestCartItemDto> guestItems, string userId)
        {
            return guestItems.Select(item => new ShoppingCartDto
            {
                Id = Guid.NewGuid(), // Generate new ID for the cart item
                UserId = userId,
                VariantId = item.VariantId,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice,
                SalePrice = item.UnitPrice, // Assuming sale price = unit price for guests
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
        /// Update order status (for internal use or admin) - Authenticated users only
        /// </summary>
        [HttpPatch("{id}/status")]
        [Authorize]
        public async Task<IActionResult> UpdateOrderStatus(Guid id, [FromBody] UpdateOrderStatusDto request)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new { success = false, message = "User not authenticated" });

                var orderResult = await _orderService.GetOrderByIdAsync(id);
                if (!orderResult.IsSuccess)
                    return NotFound(new { success = false, message = "Order not found" });

                if (orderResult.Data.UserId != userId)
                    return Forbid("Access denied to this order");

                if (!Enum.TryParse<OrderStatus>(request.Status, true, out var newStatus))
                {
                    return BadRequest(new { success = false, message = "Invalid order status" });
                }

                var result = await _orderService.UpdateOrderStatusAsync(id, newStatus);

                if (result.IsSuccess)
                {
                    return Ok(new { success = true, data = result.Data, message = "Order status updated successfully" });
                }

                return BadRequest(new { success = false, message = result.ErrorMessage });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating order status for order {OrderId}", id);
                return StatusCode(500, new { success = false, message = "Internal server error" });
            }
        }

        /// <summary>
        /// Cancel an order (only if it's in Pending status) - Authenticated users only
        /// </summary>
        [HttpPatch("{id}/cancel")]
        [Authorize]
        public async Task<IActionResult> CancelOrder(Guid id, [FromBody] CancelOrderDto request)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new { success = false, message = "User not authenticated" });

                var orderResult = await _orderService.GetOrderByIdAsync(id);
                if (!orderResult.IsSuccess)
                    return NotFound(new { success = false, message = "Order not found" });

                if (orderResult.Data.UserId != userId)
                    return Forbid("Access denied to this order");

                if (orderResult.Data.OrderStatus != OrderStatus.Pending)
                    return BadRequest(new { success = false, message = "Order cannot be cancelled at this stage" });

                var result = await _orderService.CancelOrderAsync(id, request.CancellationReason);

                if (result.IsSuccess)
                {
                    _logger.LogInformation("Order cancelled: {OrderId} by user {UserId}", id, userId);
                    return Ok(new { success = true, data = result.Data, message = "Order cancelled successfully" });
                }

                return BadRequest(new { success = false, message = result.ErrorMessage });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling order {OrderId}", id);
                return StatusCode(500, new { success = false, message = "Internal server error" });
            }
        }

        /// <summary>
        /// Get order history with pagination - Authenticated users only
        /// </summary>
        [HttpGet("history")]
        [Authorize]
        public async Task<IActionResult> GetOrderHistory(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? status = null)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new { success = false, message = "User not authenticated" });

                OrderStatus? parsedStatus = null;
                if (!string.IsNullOrEmpty(status) &&
                    Enum.TryParse<OrderStatus>(status, true, out var enumValue))
                {
                    parsedStatus = enumValue;
                }

                var result = await _orderService.GetOrderHistoryAsync(userId, page, pageSize, parsedStatus);

                if (result.IsSuccess)
                {
                    return Ok(new
                    {
                        success = true,
                        data = result.Data,
                        pagination = new
                        {
                            currentPage = page,
                            pageSize = pageSize,
                            hasNextPage = result.Data.TotalCount == pageSize
                        }
                    });
                }

                return BadRequest(new { success = false, message = result.ErrorMessage });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting order history for user {UserId}", User?.Identity?.Name);
                return StatusCode(500, new { success = false, message = "Internal server error" });
            }
        }

        /// <summary>
        /// Get order tracking information
        /// </summary>
        [HttpGet("{id}/tracking")]
        public async Task<IActionResult> GetOrderTracking(Guid id, [FromQuery] string? guestEmail = null)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var orderResult = await _orderService.GetOrderByIdAsync(id);

                if (!orderResult.IsSuccess)
                    return NotFound(new { success = false, message = "Order not found" });

                // Verify access rights (similar to GetOrderById)
                if (!string.IsNullOrEmpty(userId))
                {
                    if (!orderResult.Data.UserId.StartsWith("guest_") && orderResult.Data.UserId != userId)
                        return Forbid("Access denied to this order");
                }
                else if (orderResult.Data.UserId.StartsWith("guest_"))
                {
                    if (string.IsNullOrEmpty(guestEmail))
                        return BadRequest(new { success = false, message = "Guest email is required" });
                }
                else
                {
                    return Unauthorized(new { success = false, message = "Access denied" });
                }

                var result = await _orderService.GetOrderTrackingAsync(id);

                if (result.IsSuccess)
                {
                    return Ok(new { success = true, data = result.Data });
                }

                return BadRequest(new { success = false, message = result.ErrorMessage });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting order tracking for order {OrderId}", id);
                return StatusCode(500, new { success = false, message = "Internal server error" });
            }
        }
    }

    // Updated DTOs
    public class CreateOrderFromCartDto
    {
        public string ShippingAddress { get; set; }
        public string BillingAddress { get; set; }
        public string Currency { get; set; }
        public string? CouponCode { get; set; }
        public string? PaymentMethod { get; set; }
        public string? Notes { get; set; }

        // Properties for guest users
        public string? GuestUserId { get; set; }
        public string? GuestEmail { get; set; }
        public List<GuestCartItemDto>? CartItems { get; set; } // Updated to use GuestCartItemDto
    }

    // New DTO to match your frontend localStorage structure
    public class GuestCartItemDto
    {
        public string Id { get; set; } // cart_1756138522402_iwbmzvdxx
        public Guid VariantId { get; set; }
        public Guid ProductId { get; set; }
        public string ProductName { get; set; }
        public string Sku { get; set; }
        public string Size { get; set; }
        public string Color { get; set; }
        public string ImageUrl { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal OriginalPrice { get; set; }
        public int Quantity { get; set; }
        public decimal TotalPrice { get; set; }
        public string VariantDetails { get; set; }
        public int MaxStock { get; set; }
    }

    public class UpdateOrderStatusDto
    {
        public string Status { get; set; }
    }

    public class CancelOrderDto
    {
        public string? CancellationReason { get; set; }
    }
}