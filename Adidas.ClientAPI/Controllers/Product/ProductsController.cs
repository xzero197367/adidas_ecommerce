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
        public ProductsController(IProductService productService)
        {
            _productService = productService;
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
      

    }
}

//    ProductController
//    
//    • - GetSalesProducts
//    • - GetProductsByCategory
//    • - GetProductVariantById
//    • - GetImagesByProductVariantId
//    • - GetProductsYouMayLike