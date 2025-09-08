using Adidas.Application.Contracts.ServicesContracts.Separator;
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

        [HttpGet("GetMainCategoriesByType/{Type}")]
        public async Task<IActionResult> GetMainCategoriesByType(string Type)
        {
            var categories = await _categoryService.GetMainCategoriesByType(Type);
            return Ok(categories);
        }

        [HttpGet("GetAllMainCategories")]
        public async Task<IActionResult> GetAllMainCategories()
        {
            var categories = await _categoryService.GetFilteredCategoriesAsync("Main", "Active", "");
            return Ok(categories);
        }

        [HttpGet("GetSubCategoriesByCategoryId/{id}")]
        public async Task<IActionResult> GetSubCategoriesByCategoryId(Guid id)
        {
            var subCategories = await _categoryService.GetSubCategoriesByCategoryId(id);
            return Ok(subCategories);
        }

        [HttpGet("GetSubCategoriesByCategorySlug/{slug}")]
        public async Task<IActionResult> GetSubCategoriesByCategorySlug(string slug)
        {
            var subCategories = await _categoryService.GetSubCategoriesByCategorySlug(slug);
            return Ok(subCategories);
        }

        // NEW ENDPOINTS MISSING FROM YOUR CONTROLLER:

        [HttpGet("GetCategoryDetails/{id}")]
        public async Task<IActionResult> GetCategoryDetails(Guid id)
        {
            var category = await _categoryService.GetCategoryDetailsAsync(id);
            if (category == null)
                return NotFound();
            return Ok(category);
        }
        [HttpGet("GetCategoryDetailsBySlug/{slug}")]
        public async Task<IActionResult> GetCategoryDetailsBySlug(string slug)
        {
            var category = await _categoryService.GetCategoryBySlugAsync(slug);
            if (category == null)
                return NotFound();
            return Ok(category);
        }

        [HttpGet("GetFilteredCategories")]
        public async Task<IActionResult> GetFilteredCategories(
            [FromQuery] string? categoryType = null,
            [FromQuery] string? statusFilter = null,
            [FromQuery] string? searchTerm = null)
        {
            var categories = await _categoryService.GetFilteredCategoriesAsync(categoryType, statusFilter, searchTerm);
            return Ok(categories);
        }

        // This endpoint is used by the Angular service for searching categories
        [HttpGet("SearchCategories")]
        public async Task<IActionResult> SearchCategories([FromQuery] string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return Ok(new List<object>());

            var categories = await _categoryService.GetFilteredCategoriesAsync(null, "Active", searchTerm);
            return Ok(categories);
        }
    }
}