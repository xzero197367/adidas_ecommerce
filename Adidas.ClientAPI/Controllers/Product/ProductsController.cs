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
      
      

    }
}

//    ProductController
//    
//    • - GetSalesProducts
//    • - GetProductsByCategory
//    • - GetProductVariantById
//    • - GetImagesByProductVariantId
//    • - GetProductsYouMayLike