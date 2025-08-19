using Adidas.Application.Contracts.ServicesContracts.Feature;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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

            return Ok(topProducts);
        }
    }
}
