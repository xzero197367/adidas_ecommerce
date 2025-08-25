using Adidas.Application.Contracts.ServicesContracts.Feature;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace Adidas.ClientAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CartController : ControllerBase
    {
        private readonly IShoppingCartService _cartService;
        private readonly Adidas.Context.AdidasDbContext _dbContext;

        public CartController(IShoppingCartService cartService, Adidas.Context.AdidasDbContext dbContext)
        {
            _cartService = cartService;
            _dbContext = dbContext;
        }

        /// <summary>
        /// Get all cart items for the authenticated user
        /// </summary>
        [HttpGet]
        [Authorize]
        [HttpGet("cart")]
        public async Task<IActionResult> GetCart()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { success = false, message = "User not authorized" });

            var result = await _cartService.GetCartItemsByUserIdAsync(userId);
            if (!result.IsSuccess)
                return BadRequest(new { success = false, message = result.ErrorMessage });

            var cartItems = result.Data.Select(item =>
            {
                var product = item.Variant?.Product;
                var variant = item.Variant;

                var unitPrice = (product?.Price ?? 0) + (variant?.PriceAdjustment ?? 0);
                var totalPrice = unitPrice * item.Quantity;

                return new
                {
                    id = item.Id,
                    variantId = variant?.Id,
                    quantity = item.Quantity,
                    unitPrice,
                    totalPrice,
                    productName = product?.Name,
                    variantDetails = $"{variant?.Size ?? "N/A"} - {variant?.Color ?? "N/A"}",
                    addedAt = item.CreatedAt,
                    variant = variant == null ? null : new
                    {
                        id = variant.Id,
                        sku = variant.Sku,
                        size = variant.Size,
                        color = variant.Color,
                        imageUrl = variant.Images?.FirstOrDefault()?.ImageUrl,
                        stockQuantity = variant.StockQuantity,
                        priceAdjustment = variant.PriceAdjustment,
                        productId = variant.ProductId,
                        product = product == null ? null : new
                        {
                            id = product.Id,
                            name = product.Name,
                            price = product.Price,
                            imageUrl = product.Images?.FirstOrDefault()?.ImageUrl
                        }
                    }
                };
            });

            return Ok(new { success = true, data = cartItems });
        }


        /// <summary>
        /// Add item to cart or update quantity if exists
        /// </summary>
        [HttpPost("add")]
        //[Authorize]
        public async Task<IActionResult> AddToCart([FromBody] AddToCartRequest request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            try
            {
                // Check if variant exists and is available
                var variant = await _dbContext.ProductVariants
                    .Include(v => v.Product)
                    .FirstOrDefaultAsync(v => v.Id.ToString() == request.VariantId);

                if (variant == null)
                    return BadRequest(new { success = false, message = "Product variant not found" });

                if (variant.StockQuantity < request.Quantity)
                    return BadRequest(new { success = false, message = "Insufficient stock available" });

                // Check if item already exists in cart
                var existingCartItem = await _dbContext.ShoppingCarts
                    .FirstOrDefaultAsync(c => c.UserId == userId && c.VariantId.ToString() == request.VariantId);

                if (existingCartItem != null)
                {
                    // Update quantity
                    var newQuantity = existingCartItem.Quantity + request.Quantity;
                    if (variant.StockQuantity < newQuantity)
                        return BadRequest(new { success = false, message = "Insufficient stock available" });

                    existingCartItem.Quantity = newQuantity;
                    existingCartItem.UpdatedAt = DateTime.UtcNow;
                }
                else
                {
                    // Add new item
                    
                    var cartItem = new Adidas.Models.Feature.ShoppingCart
                    {
                        UserId = userId,
                        VariantId = Guid.Parse(request.VariantId),
                        Quantity = request.Quantity,
                        AddedAt = DateTime.UtcNow
                    };
                    _dbContext.ShoppingCarts.Add(cartItem);
                }

                await _dbContext.SaveChangesAsync();
                return Ok(new { success = true, message = "Item added to cart successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "An error occurred while adding to cart" });
            }
        }

        /// <summary>
        /// Update cart item quantity
        /// </summary>
        [HttpPut("update")]
        [Authorize]
        public async Task<IActionResult> UpdateCartItem([FromBody] UpdateCartItemRequest request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            try
            {
                var cartItem = await _dbContext.ShoppingCarts
                    .Include(c => c.Variant)
                    .FirstOrDefaultAsync(c => c.Id.ToString() == request.CartItemId && c.UserId == userId);

                if (cartItem == null)
                    return NotFound(new { success = false, message = "Cart item not found" });

                if (request.Quantity <= 0)
                {
                    _dbContext.ShoppingCarts.Remove(cartItem);
                }
                else
                {
                    if (cartItem.Variant.StockQuantity < request.Quantity)
                        return BadRequest(new { success = false, message = "Insufficient stock available" });

                    cartItem.Quantity = request.Quantity;
                    cartItem.UpdatedAt = DateTime.UtcNow;
                }

                await _dbContext.SaveChangesAsync();
                return Ok(new { success = true, message = "Cart updated successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "An error occurred while updating cart" });
            }
        }

        /// <summary>
        /// Remove item from cart
        /// </summary>
        [HttpDelete("remove/{cartItemId}")]
        [Authorize]
        public async Task<IActionResult> RemoveFromCart(Guid cartItemId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            try
            {
                var cartItem = await _dbContext.ShoppingCarts
                    .FirstOrDefaultAsync(c => c.Id == cartItemId && c.UserId == userId);

                if (cartItem == null)
                    return NotFound(new { success = false, message = "Cart item not found" });

                _dbContext.ShoppingCarts.Remove(cartItem);
                await _dbContext.SaveChangesAsync();

                return Ok(new { success = true, message = "Item removed from cart successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "An error occurred while removing from cart" });
            }
        }

        /// <summary>
        /// Clear entire cart
        /// </summary>
        [HttpDelete("clear")]
        [Authorize]
        public async Task<IActionResult> ClearCart()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            try
            {
                var cartItems = await _dbContext.ShoppingCarts
                    .Where(c => c.UserId == userId)
                    .ToListAsync();

                _dbContext.ShoppingCarts.RemoveRange(cartItems);
                await _dbContext.SaveChangesAsync();

                return Ok(new { success = true, message = "Cart cleared successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "An error occurred while clearing cart" });
            }
        }

        /// <summary>
        /// Sync cart items (used when user logs in with existing cart)
        /// </summary>
        [HttpPost("sync")]
        [Authorize]
        public async Task<IActionResult> SyncCart([FromBody] SyncCartRequest request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            try
            {
                // Get existing cart items
                var existingItems = await _dbContext.ShoppingCarts
                    .Where(c => c.UserId == userId)
                    .ToListAsync();

                // Process each item from the request
                foreach (var item in request.Items)
                {
                    var variant = await _dbContext.ProductVariants
                        .FirstOrDefaultAsync(v => v.Id == Guid.Parse(item.VariantId));

                    if (variant == null || variant.StockQuantity < item.Quantity)
                        continue; // Skip invalid items

                    var existingItem = existingItems.FirstOrDefault(e => e.VariantId == Guid.Parse(item.VariantId));

                    if (existingItem != null)
                    {
                        // Update existing item with higher quantity
                        existingItem.Quantity = Math.Max(existingItem.Quantity, item.Quantity);
                        existingItem.UpdatedAt = DateTime.UtcNow;
                    }
                    else
                    {
                        // Add new item
                        var cartItem = new Adidas.Models.Feature.ShoppingCart
                        {
                            UserId = userId,
                            VariantId = Guid.Parse(item.VariantId),
                            Quantity = item.Quantity,
                            AddedAt = DateTime.UtcNow
                        };
                        _dbContext.ShoppingCarts.Add(cartItem);
                    }
                }

                await _dbContext.SaveChangesAsync();
                return Ok(new { success = true, message = "Cart synced successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "An error occurred while syncing cart" });
            }
        }

        /// <summary>
        /// Validate cart items (check stock, prices, availability)
        /// </summary>
        [HttpPost("validate")]
        [Authorize]
        public async Task<IActionResult> ValidateCart([FromBody] ValidateCartRequest request)
        {
            try
            {
                var variantIds = request.VariantIds.Select(id => Guid.Parse(id)).ToList();

                var variants = await _dbContext.ProductVariants
                    .Include(v => v.Product)
                    .Where(v => variantIds.Contains(v.Id))
                    .Select(v => new
                    {
                        variantId = v.Id.ToString(),
                        isAvailable = v.StockQuantity > 0 && v.IsActive && !v.IsDeleted,
                        stockQuantity = v.StockQuantity,
                        currentPrice = v.Product.Price + v.PriceAdjustment
                    })
                    .ToListAsync();

                return Ok(new { success = true, data = variants });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "An error occurred while validating cart" });
            }
        }

        /// <summary>
        /// Get cart summary (item count, total, etc.)
        /// </summary>
        [HttpGet("summary")]
        [Authorize]
        public async Task<IActionResult> GetCartSummary()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            try
            {
                var cartItems = await _dbContext.ShoppingCarts
                    .Include(c => c.Variant)
                    .ThenInclude(v => v.Product)
                    .Where(c => c.UserId == userId)
                    .ToListAsync();

                var summary = new
                {
                    itemCount = cartItems.Count,
                    totalItems = cartItems.Sum(c => c.Quantity),
                    subtotal = cartItems.Sum(c => (c.Variant.Product.Price + c.Variant.PriceAdjustment) * c.Quantity),
                    currency = "USD"
                };

                return Ok(new { success = true, data = summary });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "An error occurred while getting cart summary" });
            }
        }

        /// <summary>
        /// Get top-selling products (based on OrderItems aggregation)
        /// </summary>
        [HttpGet("top-selling")]
        public async Task<IActionResult> GetAllTopSellingProducts([FromQuery] int count = 10)
        {
            var topProducts = await _dbContext.OrderItems
                .GroupBy(o => o.Variant.Product)
                .Select(g => new
                {
                    Product = g.Key,
                    TotalSold = g.Sum(x => x.Quantity)
                })
                .OrderByDescending(x => x.TotalSold)
                .Take(count)
                .Select(x => new
                {
                    x.Product.Id,
                    x.Product.Name,
                    x.Product.Price,
                    x.Product.SalePrice,
                    x.TotalSold
                })
                .ToListAsync();

            return Ok(new { success = true, data = topProducts });
        }
    }

    // Request DTOs
    public class AddToCartRequest
    {
        public required string VariantId { get; set; }
        public int Quantity { get; set; } = 1;
    }

    public class UpdateCartItemRequest
    {
        public required string CartItemId { get; set; }
        public int Quantity { get; set; }
    }

    public class SyncCartRequest
    {
        public List<SyncCartItem> Items { get; set; } = new();
    }

    public class SyncCartItem
    {
        public required string VariantId { get; set; }
        public int Quantity { get; set; }
    }

    public class ValidateCartRequest
    {
        public List<string> VariantIds { get; set; } = new();
    }
}