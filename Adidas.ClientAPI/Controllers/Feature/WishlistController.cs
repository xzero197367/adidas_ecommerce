using Adidas.Application.Contracts.ServicesContracts.Feature;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Models.Feature;

namespace Adidas.ClientAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WishlistController : ControllerBase
    {
        private readonly IWishListService _wishlistService;
        private readonly Adidas.Context.AdidasDbContext _dbContext;

        public WishlistController(IWishListService wishlistService, Adidas.Context.AdidasDbContext dbContext)
        {
            _wishlistService = wishlistService;
            _dbContext = dbContext;
        }

        /// <summary>
        /// Get all items in the authenticated user's wishlist
        /// </summary>
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetWishlist()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            try
            {
                var wishlistItems = await _dbContext.Wishlists
                    .Include(w => w.Product)
                    .ThenInclude(p => p.Variants)
                    .Where(w => w.UserId == userId && !w.IsDeleted)
                    .OrderByDescending(w => w.AddedAt)
                    .Select(w => new
                    {
                        id = w.Id,
                        productId = w.ProductId,
                        addedAt = w.AddedAt,
                        createdAt = w.CreatedAt,
                        product = new
                        {
                            id = w.Product.Id,
                            name = w.Product.Name,
                            price = w.Product.Price,
                            originalPrice = w.Product.SalePrice,
                            imageUrl = w.Product.Images.FirstOrDefault().ImageUrl,
                            brand = w.Product.Brand,
                            category = w.Product.Category,
                            isAvailable = w.Product.IsActive && !w.Product.IsDeleted,
                            variants = w.Product.Variants
                                .Where(v => v.IsActive && !v.IsDeleted)
                                .Select(v => new
                                {
                                    id = v.Id,
                                    sku = v.Sku,
                                    size = v.Size,
                                    color = v.Color,
                                    imageUrl = v.ImageUrl,
                                    stockQuantity = v.StockQuantity,
                                    priceAdjustment = v.PriceAdjustment,
                                    isAvailable = v.StockQuantity > 0
                                })
                        }
                    })
                    .ToListAsync();

                return Ok(new { success = true, data = wishlistItems });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "An error occurred while fetching wishlist" });
            }
        }

        /// <summary>
        /// Add product to wishlist
        /// </summary>
        [HttpPost("add")]
        [Authorize]
        public async Task<IActionResult> AddToWishlist([FromBody] AddToWishlistRequest request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            try
            {
                // Check if product exists
                var product = await _dbContext.Products
                    .FirstOrDefaultAsync(p => p.Id == request.ProductId && p.IsActive && !p.IsDeleted);

                if (product == null)
                    return BadRequest(new { success = false, message = "Product not found" });

                // Check if already in wishlist
                var existingItem = await _dbContext.Wishlists
                    .FirstOrDefaultAsync(w => w.UserId == userId && w.ProductId == request.ProductId && !w.IsDeleted);

                if (existingItem != null)
                    return BadRequest(new { success = false, message = "Product already in wishlist" });

                // Add to wishlist
                var wishlistItem = new Wishlist
                {
                    UserId = userId,
                    ProductId = request.ProductId,
                    AddedAt = DateTime.UtcNow
                };

                _dbContext.Wishlists.Add(wishlistItem);
                await _dbContext.SaveChangesAsync();

                return Ok(new { success = true, message = "Product added to wishlist successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "An error occurred while adding to wishlist" });
            }
        }

        /// <summary>
        /// Remove product from wishlist
        /// </summary>
        [HttpDelete("{productId}")]
        [Authorize]
        public async Task<IActionResult> RemoveFromWishlist(Guid productId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            try
            {
                var wishlistItem = await _dbContext.Wishlists
                    .FirstOrDefaultAsync(w => w.UserId == userId && w.ProductId == productId && !w.IsDeleted);

                if (wishlistItem == null)
                    return NotFound(new { success = false, message = "Wishlist item not found" });

                // Soft delete
                wishlistItem.IsDeleted = true;
                wishlistItem.UpdatedAt = DateTime.UtcNow;

                await _dbContext.SaveChangesAsync();

                return Ok(new { success = true, message = "Product removed from wishlist successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "An error occurred while removing from wishlist" });
            }
        }

        /// <summary>
        /// Clear entire wishlist
        /// </summary>
        [HttpDelete("clear")]
        [Authorize]
        public async Task<IActionResult> ClearWishlist()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            try
            {
                var wishlistItems = await _dbContext.Wishlists
                    .Where(w => w.UserId == userId && !w.IsDeleted)
                    .ToListAsync();

                foreach (var item in wishlistItems)
                {
                    item.IsDeleted = true;
                    item.UpdatedAt = DateTime.UtcNow;
                }

                await _dbContext.SaveChangesAsync();

                return Ok(new { success = true, message = "Wishlist cleared successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "An error occurred while clearing wishlist" });
            }
        }

        /// <summary>
        /// Sync wishlist (used when user logs in)
        /// </summary>
        [HttpPost("sync")]
        [Authorize]
        public async Task<IActionResult> SyncWishlist([FromBody] SyncWishlistRequest request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            try
            {
                var productIds = request.ProductIds.Select(id => Guid.Parse(id)).ToList();

                // Get existing wishlist items
                var existingItems = await _dbContext.Wishlists
                    .Where(w => w.UserId == userId && !w.IsDeleted)
                    .Select(w => w.ProductId)
                    .ToListAsync();

                // Add new items that don't exist
                var newProductIds = productIds.Except(existingItems).ToList();

                if (newProductIds.Any())
                {
                    // Verify products exist
                    var validProductIds = await _dbContext.Products
                        .Where(p => newProductIds.Contains(p.Id) && p.IsActive && !p.IsDeleted)
                        .Select(p => p.Id)
                        .ToListAsync();

                    var wishlistItems = validProductIds.Select(productId => new Wishlist
                    {
                        UserId = userId,
                        ProductId = productId,
                        AddedAt = DateTime.UtcNow
                    });

                    _dbContext.Wishlists.AddRange(wishlistItems);
                    await _dbContext.SaveChangesAsync();
                }

                return Ok(new { success = true, message = "Wishlist synced successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "An error occurred while syncing wishlist" });
            }
        }

        /// <summary>
        /// Validate wishlist items (check availability, prices)
        /// </summary>
        [HttpPost("validate")]
        [Authorize]
        public async Task<IActionResult> ValidateWishlist([FromBody] ValidateWishlistRequest request)
        {
            try
            {
                var productIds = request.ProductIds.Select(id => Guid.Parse(id)).ToList();

                var products = await _dbContext.Products
                    .Include(p => p.Variants)
                    .Where(p => productIds.Contains(p.Id))
                    .Select(p => new
                    {
                        productId = p.Id.ToString(),
                        isAvailable = p.IsActive && !p.IsDeleted && p.Variants.Any(v => v.StockQuantity > 0),
                        currentPrice = p.Price,
                        stockQuantity = p.Variants.Sum(v => v.StockQuantity),
                        hasVariants = p.Variants.Any(v => v.IsActive && !v.IsDeleted)
                    })
                    .ToListAsync();

                return Ok(new { success = true, data = products });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "An error occurred while validating wishlist" });
            }
        }

        /// <summary>
        /// Toggle product in wishlist (add if not exists, remove if exists)
        /// </summary>
        [HttpPost("toggle")]
        [Authorize]
        public async Task<IActionResult> ToggleWishlist([FromBody] ToggleWishlistRequest request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            try
            {
                var existingItem = await _dbContext.Wishlists
                    .FirstOrDefaultAsync(w => w.UserId == userId && w.ProductId == request.ProductId && !w.IsDeleted);

                if (existingItem != null)
                {
                    // Remove from wishlist
                    existingItem.IsDeleted = true;
                    existingItem.UpdatedAt = DateTime.UtcNow;
                    await _dbContext.SaveChangesAsync();

                    return Ok(new { success = true, message = "Product removed from wishlist", inWishlist = false });
                }
                else
                {
                    // Check if product exists
                    var product = await _dbContext.Products
                        .FirstOrDefaultAsync(p => p.Id == request.ProductId && p.IsActive && !p.IsDeleted);

                    if (product == null)
                        return BadRequest(new { success = false, message = "Product not found" });

                    // Add to wishlist
                    var wishlistItem = new Wishlist
                    {
                        UserId = userId,
                        ProductId = request.ProductId,
                        AddedAt = DateTime.UtcNow
                    };

                    _dbContext.Wishlists.Add(wishlistItem);
                    await _dbContext.SaveChangesAsync();

                    return Ok(new { success = true, message = "Product added to wishlist", inWishlist = true });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "An error occurred while toggling wishlist" });
            }
        }

        /// <summary>
        /// Check if product is in user's wishlist
        /// </summary>
        [HttpGet("check/{productId}")]
        [Authorize]
        public async Task<IActionResult> CheckInWishlist(Guid productId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            try
            {
                var inWishlist = await _dbContext.Wishlists
                    .AnyAsync(w => w.UserId == userId && w.ProductId == productId && !w.IsDeleted);

                return Ok(new { success = true, inWishlist });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "An error occurred while checking wishlist" });
            }
        }

        /// <summary>
        /// Get wishlist statistics
        /// </summary>
        [HttpGet("stats")]
        [Authorize]
        public async Task<IActionResult> GetWishlistStats()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            try
            {
                var wishlistItems = await _dbContext.Wishlists
                    .Include(w => w.Product)
                    .ThenInclude(p => p.Variants)
                    .Where(w => w.UserId == userId && !w.IsDeleted)
                    .ToListAsync();

                var availableItems = wishlistItems.Where(w =>
                    w.Product.IsActive &&
                    !w.Product.IsDeleted &&
                    w.Product.Variants.Any(v => v.StockQuantity > 0)
                ).ToList();

                var stats = new
                {
                    totalItems = wishlistItems.Count,
                    availableItems = availableItems.Count,
                    unavailableItems = wishlistItems.Count - availableItems.Count,
                    totalValue = availableItems.Sum(w => w.Product.Price),
                    averagePrice = availableItems.Any() ? availableItems.Average(w => w.Product.Price) : 0
                };

                return Ok(new { success = true, data = stats });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "An error occurred while getting wishlist stats" });
            }
        }

        /// <summary>
        /// Move product from wishlist to cart
        /// </summary>
        [HttpPost("move-to-cart")]
        [Authorize]
        public async Task<IActionResult> MoveToCart([FromBody] MoveToCartRequest request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            try
            {
                // Check if product is in wishlist
                var wishlistItem = await _dbContext.Wishlists
                    .FirstOrDefaultAsync(w => w.UserId == userId && w.ProductId == request.ProductId && !w.IsDeleted);

                if (wishlistItem == null)
                    return NotFound(new { success = false, message = "Product not found in wishlist" });

                // Get product variant
                var variant = await _dbContext.ProductVariants
                    .FirstOrDefaultAsync(v => v.Id == request.VariantId && v.ProductId == request.ProductId);

                if (variant == null)
                    return BadRequest(new { success = false, message = "Product variant not found" });

                if (variant.StockQuantity < request.Quantity)
                    return BadRequest(new { success = false, message = "Insufficient stock available" });

                // Check if already in cart
                var existingCartItem = await _dbContext.ShoppingCarts
                    .FirstOrDefaultAsync(c => c.UserId == userId && c.VariantId == request.VariantId);

                if (existingCartItem != null)
                {
                    // Update quantity in cart
                    var newQuantity = existingCartItem.Quantity + request.Quantity;
                    if (variant.StockQuantity < newQuantity)
                        return BadRequest(new { success = false, message = "Insufficient stock for requested quantity" });

                    existingCartItem.Quantity = newQuantity;
                    existingCartItem.UpdatedAt = DateTime.UtcNow;
                }
                else
                {
                    // Add new cart item
                    var cartItem = new Adidas.Models.Feature.ShoppingCart
                    {
                        UserId = userId,
                        VariantId = request.VariantId,
                        Quantity = request.Quantity,
                        AddedAt = DateTime.UtcNow
                    };
                    _dbContext.ShoppingCarts.Add(cartItem);
                }

                // Remove from wishlist
                wishlistItem.IsDeleted = true;
                wishlistItem.UpdatedAt = DateTime.UtcNow;

                await _dbContext.SaveChangesAsync();

                return Ok(new { success = true, message = "Product moved to cart successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "An error occurred while moving to cart" });
            }
        }
    }

    // Request DTOs
    public class AddToWishlistRequest
    {
        public required Guid ProductId { get; set; }
    }

    public class SyncWishlistRequest
    {
        public List<string> ProductIds { get; set; } = new();
    }

    public class ValidateWishlistRequest
    {
        public List<string> ProductIds { get; set; } = new();
    }

    public class ToggleWishlistRequest
    {
        public required Guid ProductId { get; set; }
    }

    public class MoveToCartRequest
    {
        public required Guid ProductId { get; set; }
        public required Guid VariantId { get; set; }
        public int Quantity { get; set; } = 1;
    }
}