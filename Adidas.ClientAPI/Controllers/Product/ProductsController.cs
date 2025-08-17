using Adidas.Application.Contracts.ServicesContracts.Main;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Adidas.ClientAPI.Controllers.Product
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;
        private readonly IProductVariantService _productVariantService;
        public ProductsController(IProductService productService, IProductVariantService productVariantService)
        {
            _productService = productService;
            _productVariantService = productVariantService;
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
            var productVariants = await _productService.GetProductWithVariantsAsync(id);
            return Ok(productVariants);

        }

        [HttpGet("GetProductVariantById/{id}")]
        public async Task<IActionResult> GetProductVariantById(Guid id)
        {
            var productVariant = await _productVariantService.GetByIdAsync(id);        
            return Ok(productVariant);

        }
       

    }
}

//    ProductController
//    
//   
//   
//    • - GetProductVariantById
//    • - GetImagesByProductVariantId
//    • - GetProductsYouMayLike