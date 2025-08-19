using Adidas.Application.Contracts.ServicesContracts.Feature;
using Microsoft.AspNetCore.Mvc;

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
        public async Task<IActionResult> GetAllProductsInWishlist(string userId)
        {
            var result = await _wishlistService.GetWishlistByUserIdAsync(userId);
            if (!result.IsSuccess) return BadRequest(result.ErrorMessage);

            return Ok(result.Data);
        }
    }
}
