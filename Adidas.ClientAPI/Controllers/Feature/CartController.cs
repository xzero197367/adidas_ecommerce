// Controllers/ShoppingCartController.cs
using Adidas.Application.Contracts.ServicesContracts.Feature;
using Adidas.DTOs.Feature.ShoppingCartDTOS;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace Adidas.ClientAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ShoppingCartController : ControllerBase
    {
        private readonly IShoppingCartService _cartService;
        private readonly Adidas.Context.AdidasDbContext _dbContext;

        public ShoppingCartController(IShoppingCartService cartService, Adidas.Context.AdidasDbContext dbContext)
        {
            _cartService = cartService;
            _dbContext = dbContext;
        }

        /// <summary>
        /// Get all cart items for a user
        /// </summary>
        [HttpGet("{userId}")]
        public async Task<IActionResult> GetAllCartItems(string userId)
        {
            var result = await _cartService.GetCartItemsByUserIdAsync(userId);
            if (!result.IsSuccess) return BadRequest(result.ErrorMessage);

            return Ok(result.Data);
        }

        /// <summary>
        /// Add item to cart
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> AddToCart([FromBody] ShoppingCartCreateDto createDto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var result = await _cartService.AddToCartAsync(createDto);
            if (!result.IsSuccess) return BadRequest(result.ErrorMessage);

            return Ok(result.Data);
        }

        /// <summary>
        /// Update cart item quantity
        /// </summary>
        [HttpPut("{itemId}")]
        public async Task<IActionResult> UpdateCartItem(Guid itemId, [FromBody] ShoppingCartUpdateDto updateDto)
        {
            updateDto.Id = itemId;
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var result = await _cartService.UpdateCartItemQuantityAsync(updateDto);
            if (!result.IsSuccess) return BadRequest(result.ErrorMessage);

            return Ok(result.Data);
        }
        [HttpPost("add")]
       

      

        /// <summary>
        /// Remove item from cart
        /// </summary>
        [HttpDelete("{userId}/{variantId}")]
        public async Task<IActionResult> RemoveFromCart(string userId, Guid variantId)
        {
            var result = await _cartService.RemoveFromCartAsync(userId, variantId);
            if (!result.IsSuccess) return BadRequest(result.ErrorMessage);

            return Ok(result.Data);
        }

        /// <summary>
        /// Clear entire cart
        /// </summary>
        [HttpDelete("clear/{userId}")]
        public async Task<IActionResult> ClearCart(string userId)
        {
            var result = await _cartService.ClearCartAsync(userId);
            if (!result.IsSuccess) return BadRequest(result.ErrorMessage);

            return Ok(result.Data);
        }

        /// <summary>
        /// Get cart summary with totals
        /// </summary>
        [HttpGet("summary/{userId}")]
        public async Task<IActionResult> GetCartSummary(string userId)
        {
            var result = await _cartService.GetCartSummaryWithTaxAsync(userId);
            if (!result.IsSuccess) return BadRequest(result.ErrorMessage);

            return Ok(result.Data);
        }

        /// <summary>
        /// Sync guest cart to user account
        /// </summary>
        [HttpPost("sync/{toUserId}")]
        public async Task<IActionResult> SyncGuestCartToUser(string toUserId, [FromBody] CartSyncRequest request)
        {
            try
            {
                // Create temporary cart items for guest
                foreach (var line in request.Lines)
                {
                    await _cartService.AddToCartAsync(new ShoppingCartCreateDto
                    {
                        UserId = request.GuestId,
                        ProductVariantId = line.VariantId,
                        Quantity = line.Quantity
                    });
                }

                // Merge guest cart with user cart
                var mergeResult = await _cartService.MergeCartsAsync(request.GuestId, toUserId);
                if (!mergeResult.IsSuccess)
                    return BadRequest(mergeResult.ErrorMessage);

                return Ok(new { success = true, message = "Cart synced successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }
        /// <summary>
        /// Move item from cart to wishlist
        /// </summary>
        [HttpPost("move-to-wishlist/{userId}/{variantId}")]
        public async Task<IActionResult> MoveToWishlist(string userId, Guid variantId)
        {
            var result = await _cartService.MoveToWishlistAsync(userId, variantId);
            if (!result.IsSuccess) return BadRequest(result.ErrorMessage);

            return Ok(result.Data);
        }

        /// <summary>
        /// Get top-selling products
        /// </summary>
        [HttpGet("top-selling")]
        public async Task<IActionResult> GetTopSellingProducts([FromQuery] int count = 10)
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

            return Ok(topProducts);



          

        }

        public record CartSyncRequest(string GuestId, IEnumerable<CartLineSync> Lines);
        public record CartLineSync(Guid VariantId, int Quantity);
    }
}