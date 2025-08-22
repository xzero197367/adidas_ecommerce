using Adidas.Application.Contracts.ServicesContracts.Operation;
using Adidas.Application.Contracts.ServicesContracts.Feature;
using Adidas.Models.Operation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Adidas.ClientAPI.Controllers.Operation
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
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
        /// Get all orders for the current user
        /// </summary>
        [HttpGet("GetAllOrdersByUserId")]
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
        /// Get the current active/pending order for the user
        /// </summary>
        [HttpGet("active")]
        public async Task<IActionResult> GetActiveOrder()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new { success = false, message = "User not authenticated" });

                var result = await _orderService.GetOrdersByUserIdAsync(userId);
                if (!result.IsSuccess)
                    return BadRequest(new { success = false, message = result.ErrorMessage });

                var activeOrder = result.Data.FirstOrDefault(o => o.OrderStatus == OrderStatus.Pending);
                if (activeOrder == null)
                    return NotFound(new { success = false, message = "No active order found" });

                return Ok(new { success = true, data = activeOrder });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting active order for user {UserId}", User?.Identity?.Name);
                return StatusCode(500, new { success = false, message = "Internal server error" });
            }
        }

        /// <summary>
        /// Get order by ID (only if it belongs to the current user)
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetOrderById(Guid id)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new { success = false, message = "User not authenticated" });

                var result = await _orderService.GetOrderByIdAsync(id);
                if (!result.IsSuccess)
                    return BadRequest(new { success = false, message = result.ErrorMessage });

                // Verify the order belongs to the current user
                if (result.Data.UserId.ToString() != userId)
                    return Forbid("Access denied to this order");

                return Ok(new { success = true, data = result.Data });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting order {OrderId} for user {UserId}", id, User?.Identity?.Name);
                return StatusCode(500, new { success = false, message = "Internal server error" });
            }
        }

        /// <summary>
        /// Create order from current cart items
        /// </summary>
        [HttpPost("create-from-cart")]
        public async Task<IActionResult> CreateOrderFromCart([FromBody] CreateOrderFromCartDto request)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new { success = false, message = "User not authenticated" });

                // Validate cart has items
                var cartResult = await _cartService.GetCartItemsByUserIdAsync(userId);
                if (!cartResult.IsSuccess || !cartResult.Data.Any())
                    return BadRequest(new { success = false, message = "Cart is empty" });

                var createOrderDto = new DTOs.Operation.OrderDTOs.CreateOrderDTO
                {
                    UserId = userId,
                    ShippingAddress = request.ShippingAddress,
                    BillingAddress = request.BillingAddress,
                    Currency=request.Currancy,
                    CouponCode = request.CouponCode,
                    PaymentMethod = request.PaymentMethod,
                    Notes = request.Notes
                };

                var result = await _orderService.CreateOrderFromCartAsync(createOrderDto);

                if (result.IsSuccess)
                {
                    _logger.LogInformation("Order created from cart: {OrderId} for user {UserId}", result.Data.Id, userId);
                    return CreatedAtAction(nameof(GetOrderById),
                        new { id = result.Data.Id },
                        new { success = true, data = result.Data, message = "Order created successfully" });
                }

                return BadRequest(new { success = false, message = result.ErrorMessage });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating order from cart for user {UserId}", User?.Identity?.Name);
                return StatusCode(500, new { success = false, message = "Internal server error" });
            }
        }

        /// <summary>
        /// Update order status (for internal use or admin)
        /// </summary>
        [HttpPatch("{id}/status")]
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

                if (orderResult.Data.UserId.ToString() != userId)
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
        /// Cancel an order (only if it's in Pending status)
        /// </summary>
        [HttpPatch("{id}/cancel")]
        public async Task<IActionResult> CancelOrder(Guid id, [FromBody] CancelOrderDto request)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new { success = false, message = "User not authenticated" });

                // Verify the order belongs to the current user
                var orderResult = await _orderService.GetOrderByIdAsync(id);
                if (!orderResult.IsSuccess)
                    return NotFound(new { success = false, message = "Order not found" });

                if (orderResult.Data.UserId.ToString() != userId)
                    return Forbid("Access denied to this order");

                // Check if order can be cancelled
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
        /// Get order history with pagination
        /// </summary>
        [HttpGet("history")]
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

                // Parse string status to enum (nullable)
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
        public async Task<IActionResult> GetOrderTracking(Guid id)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new { success = false, message = "User not authenticated" });

                // Verify the order belongs to the current user
                var orderResult = await _orderService.GetOrderByIdAsync(id);
                if (!orderResult.IsSuccess)
                    return NotFound(new { success = false, message = "Order not found" });

                if (orderResult.Data.UserId.ToString() != userId)
                    return Forbid("Access denied to this order");

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

    // DTOs
    public class CreateOrderFromCartDto
    {
        public string ShippingAddress { get; set; }
        public string BillingAddress { get; set; }
        public string Currancy { get; set; }
        public string? CouponCode { get; set; }
        public string PaymentMethod { get; set; } = "card";
        public string? Notes { get; set; }
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