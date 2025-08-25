using Adidas.Application.Contracts.ServicesContracts.Main;
using Adidas.Application.Services.Main;
using Adidas.DTOs.Main.Product_DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Adidas.ClientAPI.Controllers.Product
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;
        private readonly IProductVariantService _productVariantService;
        private readonly IProductImageService _productImageService;
        private readonly IRecommendationService _recommendationService;
        public ProductsController(IProductService productService, IProductVariantService productVariantService, IProductImageService productImageService, IRecommendationService recommendationService)
        {
            _productService = productService;
            _productVariantService = productVariantService;
            _productImageService = productImageService;
            _recommendationService = recommendationService;
        }

        [HttpGet("GetAllProducts")]
        public async Task<IActionResult> GetProducts()
        {
            var products = await _productService.GetAllAsync();
            return Ok(products);
        }

        [HttpGet("GetLastAddedProducts")]
        public async Task<IActionResult> GetLastAddedProducts()
        {
            var products = await _productService.GetLastAddedProducts();
            return Ok(products);
        }

        [HttpGet("GetSalesProducts")]
        public async Task<IActionResult> GetSalesProducts()
        {
            var products = await _productService.GetSalesProducts();
            return Ok(products);
        }

        [HttpGet("GetProductsByCategoryId/{id}")]
        public async Task<IActionResult> GetProductsByCategoryId(Guid id)
        {
            var products = await _productService.GetProductsByCategoryAsync(id);
            return Ok(products);
        }

        [HttpGet("GetProductVariantsById/{id}")]
        public async Task<IActionResult> GetProductVariantsById(Guid id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var productVariants = await _productService.GetProductWithVariantsAsync(id,userId);
            return Ok(productVariants);
        }

        [HttpGet("GetProductVariantById/{id}")]
        public async Task<IActionResult> GetProductVariantById(Guid id)
        {
            var productVariant = await _productVariantService.GetByIdAsync(id);
            return Ok(productVariant);
        }

        [HttpGet("GetImagesByProductVariantId/{id}")]
        public async Task<IActionResult> GetImagesByProductVariantId(Guid id)
        {
            var productVariantImages = await _productImageService.GetImagesByVariantIdAsync(id);
            return Ok(productVariantImages);
        }

        // New search endpoint
        [HttpGet("SearchProducts")]
        public async Task<IActionResult> SearchProducts([FromQuery] string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                return BadRequest("Search term cannot be empty");
            }

            var result = await _productService.SearchProductsAsync(searchTerm);

            if (result.IsSuccess)
            {
                return Ok(result.Data);
            }

            return BadRequest(result.ErrorMessage);
        }

        [HttpGet("GetRecommendations/{productId}")]
        public async Task<IActionResult> GetRecommendations(Guid productId)
        {
            var recommendations = await _recommendationService.GetRecommendationsAsync(productId);
            return Ok(recommendations);
        }


    }
}