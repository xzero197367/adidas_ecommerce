// Controllers/WishlistController.cs
using Adidas.Application.Contracts.ServicesContracts.Feature;
using Adidas.DTOs.Feature.WishLIstDTOS;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace Adidas.ClientAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WishlistController : ControllerBase
    {
        private readonly IWishListService _wishlistService;

        public WishlistController(IWishListService wishlistService)
        {
            _wishlistService = wishlistService;
        }

        /// <summary>
        /// Get all products in a user's wishlist
        /// </summary>
        [HttpGet("{userId}")]
        public async Task<IActionResult> GetUserWishlist(string userId)
        {
            var result = await _wishlistService.GetWishlistByUserIdAsync(userId);
            if (!result.IsSuccess) return BadRequest(result.ErrorMessage);

            return Ok(result.Data);
        }

        /// <summary>
        /// Add product to wishlist
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> AddToWishlist([FromBody] WishlistCreateDto createDto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var result = await _wishlistService.AddToWishlistAsync(createDto);
            if (!result.IsSuccess) return BadRequest(result.ErrorMessage);

            return Ok(result.Data);
        }

        /// <summary>
        /// Remove product from wishlist
        /// </summary>
        [HttpDelete("{userId}/{productId}")]
        public async Task<IActionResult> RemoveFromWishlist(string userId, Guid productId)
        {
            var result = await _wishlistService.RemoveFromWishlistAsync(userId, productId);
            if (!result.IsSuccess) return BadRequest(result.ErrorMessage);

            return Ok(new { success = result.Data, message = "Item removed from wishlist" });
        }

        /// <summary>
        /// Check if product is in wishlist
        /// </summary>
        [HttpGet("check/{userId}/{productId}")]
        public async Task<IActionResult> IsInWishlist(string userId, Guid productId)
        {
            var result = await _wishlistService.IsProductInWishlistAsync(userId, productId);
            if (!result.IsSuccess) return BadRequest(result.ErrorMessage);

            return Ok(new { isInWishlist = result.Data });
        }

        /// <summary>
        /// Get wishlist count
        /// </summary>
        [HttpGet("count/{userId}")]
        public async Task<IActionResult> GetWishlistCount(string userId)
        {
            var result = await _wishlistService.GetWishlistCountAsync(userId);
            if (!result.IsSuccess) return BadRequest(result.ErrorMessage);

            return Ok(new { count = result.Data });
        }

        /// <summary>
        /// Sync guest wishlist to user account
        /// </summary>
        [HttpPost("sync/{userId}")]
        public async Task<IActionResult> SyncGuestWishlistToUser(string userId, [FromBody] WishlistSyncRequest request)
        {
            try
            {
                foreach (var productId in request.ProductIds.Distinct())
                {
                    var exists = await _wishlistService.IsProductInWishlistAsync(userId, productId);
                    if (!exists.Data) // Only add if not already in wishlist
                    {
                        await _wishlistService.AddToWishlistAsync(new WishlistCreateDto
                        {
                            UserId = userId,
                            ProductId = productId
                        });
                    }
                }

                return Ok(new { success = true, message = "Wishlist synced successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Get wishlist summary with product details
        /// </summary>
        [HttpGet("summary/{userId}")]
        public async Task<IActionResult> GetWishlistSummary(string userId)
        {
            var result = await _wishlistService.GetWishlistSummaryAsync(userId);
            if (!result.IsSuccess) return BadRequest(result.ErrorMessage);

            return Ok(result.Data);
        }

        /// <summary>
        /// Toggle product in wishlist (add if not exists, remove if exists)
        /// </summary>
        [HttpPost("toggle")]
        public async Task<IActionResult> ToggleWishlist([FromBody] WishlistCreateDto toggleDto)
        {
            var exists = await _wishlistService.IsProductInWishlistAsync(toggleDto.UserId, toggleDto.ProductId);

            if (exists.Data)
            {
                var removeResult = await _wishlistService.RemoveFromWishlistAsync(toggleDto.UserId, toggleDto.ProductId);
                return Ok(new { inWishlist = false, message = "Removed from wishlist" });
            }
            else
            {
                var addResult = await _wishlistService.AddToWishlistAsync(toggleDto);
                return Ok(new { inWishlist = true, message = "Added to wishlist" });
            }
        }
    }

    public record WishlistSyncRequest(string GuestId, IEnumerable<Guid> ProductIds);
}