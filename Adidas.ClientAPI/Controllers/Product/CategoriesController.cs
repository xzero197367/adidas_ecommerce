using Adidas.Application.Contracts.ServicesContracts.Separator;
using Adidas.Application.Services.Separator;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Adidas.ClientAPI.Controllers.Product
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriesController : ControllerBase
    {
        private readonly ICategoryService _categoryService;

        public CategoriesController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        [HttpGet("GetAllSubCategories")]
        public async Task<IActionResult> GetAllSubCategories()
        {
            var categories = await _categoryService.GetFilteredCategoriesAsync("Sub", "Active", "");
            return Ok(categories);

        }
    }
}
//    
//CategoryController
//    
//    • - FilterProducts
//    • - GetAllSubCategories
//    • - GetSubCategoriesByGender
//    • - FilterSubCategories
//Team Member: Ahmad Kh