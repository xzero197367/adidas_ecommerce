using Adidas.Application.Contracts.ServicesContracts.Separator;
using Adidas.DTOs.Main.Product_DTOs;
using Adidas.DTOs.Separator.Category_DTOs;
using Adidas.Models.Separator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Adidas.AdminDashboardMVC.Controllers.Products
{
    [Authorize(Policy = "EmployeeOrAdmin")]
    public class SubcategoriesController : Controller
    {
        private readonly ICategoryService _categoryService;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public SubcategoriesController(ICategoryService categoryService, IWebHostEnvironment webHostEnvironment)
        {
            _categoryService = categoryService;
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task<IActionResult> Index(string statusFilter, string searchTerm)
        {
            var categories = await _categoryService.GetFilteredCategoriesAsync("Sub", statusFilter, searchTerm);

            ViewData["CurrentStatus"] = statusFilter;
            ViewData["SearchTerm"] = searchTerm;

            return View(categories);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await PopulateMainCategoriesDropdown();
            return View(new CategoryCreateDto());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CategoryCreateDto model)
        {
            if (!ModelState.IsValid)
            {
                await PopulateMainCategoriesDropdown();
                if (model.ImageFile == null)
                {
                    ModelState.AddModelError("ImageUrl", "Image is Required");
                }
                return View(model);
            }

            // Validate that ParentCategoryId is provided (required for subcategory)
            if (model.ParentCategoryId == null)
            {
                ModelState.AddModelError("ParentCategoryId", "Parent category is required for subcategories.");
                await PopulateMainCategoriesDropdown();
                return View(model);
            }

            // Get the parent category to inherit its type
            var parentCategory = await _categoryService.GetCategoryDetailsAsync(model.ParentCategoryId.Value);
            if (parentCategory != null && !string.IsNullOrEmpty(parentCategory.Type))
            {
                // Convert string type to enum
                // Try parsing as integer first (most likely scenario: "0", "1", "2", "3")
                if (int.TryParse(parentCategory.Type, out int typeInt) && Enum.IsDefined(typeof(CategoryType), typeInt))
                {
                    model.Type = (CategoryType)typeInt;
                }
                // Try parsing as enum name second ("Men", "Women", "Kids", "Sports")
                else if (Enum.TryParse<CategoryType>(parentCategory.Type, true, out CategoryType parsedType))
                {
                    model.Type = parsedType;
                }
            }

            var result = await _categoryService.CreateAsync(model);

            if (!result.IsSuccess)
            {
                ModelState.AddModelError(string.Empty, result.Error);
                TempData["Error"] = result.Error;
                await PopulateMainCategoriesDropdown();
                return View(model);
            }

            TempData["Success"] = "Subcategory created successfully!";
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
        {
            var category = await _categoryService.GetCategoryToEditByIdAsync(id);

            // Ensure we're only editing subcategories
            if (category == null || category.ParentCategoryId == null)
            {
                TempData["Error"] = "Subcategory not found.";
                return RedirectToAction("Index");
            }

            await PopulateMainCategoriesDropdown();
            return View(category);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(CategoryUpdateDto model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.CategoryId = model.Id;
                await PopulateMainCategoriesDropdown();
                return View(model);
            }

            // Validate that ParentCategoryId is provided (required for subcategory)
            if (model.ParentCategoryId == null)
            {
                ModelState.AddModelError("ParentCategoryId", "Parent category is required for subcategories.");
                ViewBag.CategoryId = model.Id;
                await PopulateMainCategoriesDropdown();
                return View(model);
            }

            // Get the parent category to inherit its type
            var parentCategory = await _categoryService.GetCategoryDetailsAsync(model.ParentCategoryId.Value);
            if (parentCategory != null && !string.IsNullOrEmpty(parentCategory.Type))
            {
                // Convert string type to enum
                if (Enum.TryParse<CategoryType>(parentCategory.Type, true, out CategoryType parsedType))
                {
                    model.Type = parsedType;
                }
                else if (int.TryParse(parentCategory.Type, out int typeInt) && Enum.IsDefined(typeof(CategoryType), typeInt))
                {
                    model.Type = (CategoryType)typeInt;
                }
            }

            var result = await _categoryService.UpdateAsync(model);
            if (!result.IsSuccess)
            {
                ModelState.AddModelError(string.Empty, result.Error);
                TempData["Error"] = result.Error;
                ViewBag.CategoryId = model.Id;
                await PopulateMainCategoriesDropdown();
                return View(model);
            }

            TempData["Success"] = "Subcategory updated successfully!";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> ToggleStatus(Guid id)
        {
            var result = await _categoryService.ToggleCategoryStatusAsync(id);

            if (!result.IsSuccess)
            {
                TempData["Error"] = result.Error;
            }
            else
            {
                TempData["Success"] = "Subcategory status updated successfully.";
            }

            var referer = Request.Headers["Referer"].ToString();
            if (!string.IsNullOrEmpty(referer))
            {
                return Redirect(referer);
            }

            return RedirectToAction(nameof(Index));
        }

        private async Task PopulateMainCategoriesDropdown()
        {
            var mainCategories = await _categoryService.GetMainCategoriesAsync();
            ViewBag.ParentCategories = new SelectList(mainCategories, "Id", "Name");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id)
        {
            var result = await _categoryService.DeleteAsync(id);

            if (!result.IsSuccess)
            {
                TempData["Error"] = result.Error;
            }
            else
            {
                TempData["Success"] = "Subcategory deleted successfully!";
            }

            var referer = Request.Headers["Referer"].ToString();
            if (!string.IsNullOrEmpty(referer))
            {
                return Redirect(referer);
            }

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Details(Guid id)
        {
            var referer = Request.Headers["Referer"].ToString();
            if (id == Guid.Empty)
            {
                TempData["ErrorMessage"] = "Invalid subcategory ID provided.";

                if (!string.IsNullOrEmpty(referer))
                {
                    return Redirect(referer);
                }

                return RedirectToAction($"{nameof(Index)}");
            }

            try
            {
                var categoryDto = await _categoryService.GetCategoryDetailsAsync(id);

                if (categoryDto == null || categoryDto.ParentCategoryId == null)
                {
                    TempData["ErrorMessage"] = $"Subcategory with ID '{id}' not found.";
                    if (!string.IsNullOrEmpty(referer))
                    {
                        return Redirect(referer);
                    }

                    return RedirectToAction($"{nameof(Index)}");
                }

                return View(categoryDto);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] =
                    $"An unexpected error occurred while retrieving subcategory details: {ex.Message}";
                if (!string.IsNullOrEmpty(referer))
                {
                    return Redirect(referer);
                }

                return RedirectToAction($"{nameof(Index)}");
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetProductsBySubcategoryId(Guid id)
        {
            try
            {
                var categoryDto = await _categoryService.GetCategoryDetailsAsync(id);
                if (categoryDto == null || categoryDto.Products == null || categoryDto.ParentCategoryId == null)
                    return PartialView("_CategoryProductsPartial", new List<ProductDto>());

                return PartialView("_CategoryProductsPartial", categoryDto.Products);
            }
            catch
            {
                return StatusCode(500, "An error occurred while loading products.");
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetSubcategoriesByMainCategoryId(Guid mainCategoryId)
        {
            try
            {
                var result = await _categoryService.GetFilteredCategoriesAsync("Sub", "", "");
                var subcategories = result.Where(c => c.ParentCategoryId == mainCategoryId).Select(c => new
                {
                    c.Id,
                    c.Name,
                    c.Slug
                }).OrderBy(c => c.Name);

                return Ok(subcategories);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error retrieving subcategories", error = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetSubcategoriesAjax()
        {
            try
            {
                var result = await _categoryService.GetFilteredCategoriesAsync("Sub", "", "");

                var categories = result.Select(c => new
                {
                    c.Id,
                    c.Name,
                    c.Slug,
                    c.ParentCategoryId,
                    ParentCategoryName = c.ParentCategory?.Name ?? "Unknown"
                }).OrderBy(c => c.Name);

                return Ok(categories);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error retrieving subcategories", error = ex.Message });
            }
        }

        // Add this new endpoint to get parent category type
        [HttpGet]
        public async Task<IActionResult> GetParentCategoryType(Guid parentCategoryId)
        {
            try
            {
                var parentCategory = await _categoryService.GetCategoryDetailsAsync(parentCategoryId);
                if (parentCategory != null && !string.IsNullOrEmpty(parentCategory.Type))
                {
                    // Debug: Log the actual type value
                    Console.WriteLine($"Parent category type value: '{parentCategory.Type}'");

                    // Convert string type to integer for frontend
                    // Try parsing as integer first (most likely scenario: "0", "1", "2", "3")
                    if (int.TryParse(parentCategory.Type, out int typeInt) && Enum.IsDefined(typeof(CategoryType), typeInt))
                    {
                        Console.WriteLine($"Successfully parsed as integer: {typeInt}");
                        return Ok(new { Type = typeInt });
                    }
                    // Try parsing as enum name second ("Men", "Women", "Kids", "Sports")
                    else if (Enum.TryParse<CategoryType>(parentCategory.Type, true, out CategoryType parsedType))
                    {
                        Console.WriteLine($"Successfully parsed as enum: {parsedType} = {(int)parsedType}");
                        return Ok(new { Type = (int)parsedType });
                    }
                    else
                    {
                        Console.WriteLine($"Failed to parse type: '{parentCategory.Type}'");
                        return BadRequest(new { message = $"Invalid category type format: '{parentCategory.Type}'" });
                    }
                }
                Console.WriteLine("Parent category is null or has empty type");
                return NotFound(new { message = "Parent category not found or has no type" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in GetParentCategoryType: {ex.Message}");
                return BadRequest(new { message = "Error retrieving parent category type", error = ex.Message });
            }
        }
    }
}