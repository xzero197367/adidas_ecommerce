using Adidas.Application.Contracts.ServicesContracts.Separator;
using Adidas.DTOs.Main.Product_DTOs;
using Adidas.DTOs.Separator.Category_DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Adidas.AdminDashboardMVC.Controllers.Products
{
    [Authorize(Policy = "EmployeeOrAdmin")]
    public class MainCategoriesController : Controller
    {
        private readonly ICategoryService _categoryService;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public MainCategoriesController(ICategoryService categoryService, IWebHostEnvironment webHostEnvironment)
        {
            _categoryService = categoryService;
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task<IActionResult> Index(string statusFilter, string searchTerm)
        {
            var categories = await _categoryService.GetFilteredCategoriesAsync("Main", statusFilter, searchTerm);

            ViewData["CurrentStatus"] = statusFilter;
            ViewData["SearchTerm"] = searchTerm;

            return View(categories);
        }

        [HttpGet]
        public IActionResult Create()
        {
            var model = new CategoryCreateDto
            {
                ParentCategoryId = null // Ensure this is a main category
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CategoryCreateDto model)
        {
            // Force this to be a main category
            model.ParentCategoryId = null;

            if (!ModelState.IsValid)
            {
                //if (model.ImageFile == null)
                //{
                //    ModelState.AddModelError("ImageUrl", "Image is Required");
                //}
                return View(model);
             }

            //if (model.ImageFile != null && model.ImageFile.Length > 0)
            //{
            //    if (model.ImageFile.Length > 5 * 1024 * 1024)
            //    {
            //        ModelState.AddModelError("ImageUrl", "Image size should not exceed 5MB.");
            //        return View(model);
            //    }

            //    var fileName = Guid.NewGuid() + Path.GetExtension(ImageFile.FileName);
            //    var relativePath = Path.Combine("uploads", "categories", fileName);
            //    var absolutePath = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "categories");

            //    if (!Directory.Exists(absolutePath))
            //    {
            //        Directory.CreateDirectory(absolutePath);
            //    }

            //    var filePath = Path.Combine(absolutePath, fileName);
            //    using (var stream = new FileStream(filePath, FileMode.Create))
            //    {
            //        await ImageFile.CopyToAsync(stream);
            //    }

            //    model.ImageUrl = "/" + relativePath.Replace("\\", "/");
            //}

            var result = await _categoryService.CreateAsync(model);

            if (!result.IsSuccess)
            {
                ModelState.AddModelError(string.Empty, result.Error);
                TempData["Error"] = result.Error;
                return View(model);
            }

            TempData["Success"] = "Main Category created successfully!";
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
        {
            var category = await _categoryService.GetCategoryToEditByIdAsync(id);
            
            // Ensure we're only editing main categories
            if (category == null || category.ParentCategoryId != null)
            {
                TempData["Error"] = "Main Category not found.";
                return RedirectToAction("Index");
            }
            if (category.ImageUrl == null) category.ImageUrl = "";


            return View(category);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(CategoryUpdateDto model)
        {
            // Ensure this remains a main category
            model.ParentCategoryId = null;

            if (!ModelState.IsValid)
            {
                ViewBag.CategoryId = model.Id;
                return View(model);
            }

            //if (ImageFile != null && ImageFile.Length > 0)
            //{
            //    if (ImageFile.Length > 5 * 1024 * 1024)
            //    {
            //        ModelState.AddModelError("ImageUrl", "Image size should not exceed 5MB.");
            //        ViewBag.CategoryId = model.Id;
            //        return View(model);
            //    }

            //    var fileName = Guid.NewGuid() + Path.GetExtension(ImageFile.FileName);
            //    var relativePath = Path.Combine("uploads", "categories", fileName);
            //    var absolutePath = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "categories");

            //    if (!Directory.Exists(absolutePath))
            //    {
            //        Directory.CreateDirectory(absolutePath);
            //    }

            //    var filePath = Path.Combine(absolutePath, fileName);
            //    using (var stream = new FileStream(filePath, FileMode.Create))
            //    {
            //        await ImageFile.CopyToAsync(stream);
            //    }

            //    model.ImageUrl = "/" + relativePath.Replace("\\", "/");
            //}

            var result = await _categoryService.UpdateAsync(model);
            if (!result.IsSuccess)
            {
                ModelState.AddModelError(string.Empty, result.Error);
                TempData["Error"] = result.Error;
                ViewBag.CategoryId = model.Id;
                return View(model);
            }

            TempData["Success"] = "Main Category updated successfully!";
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
                TempData["Success"] = "Main Category status updated successfully.";
            }

            var referer = Request.Headers["Referer"].ToString();
            if (!string.IsNullOrEmpty(referer))
            {
                return Redirect(referer);
            }

            return RedirectToAction(nameof(Index));
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
                TempData["Success"] = "Main Category deleted successfully!";
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
                TempData["ErrorMessage"] = "Invalid main category ID provided.";

                if (!string.IsNullOrEmpty(referer))
                {
                    return Redirect(referer);
                }

                return RedirectToAction($"{nameof(Index)}");
            }

            try
            {
                var categoryDto = await _categoryService.GetCategoryDetailsAsync(id);

                if (categoryDto == null || categoryDto.ParentCategoryId != null)
                {
                    TempData["ErrorMessage"] = $"Main Category with ID '{id}' not found.";
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
                    $"An unexpected error occurred while retrieving main category details: {ex.Message}";
                if (!string.IsNullOrEmpty(referer))
                {
                    return Redirect(referer);
                }

                return RedirectToAction($"{nameof(Index)}");
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetProductsByMainCategoryId(Guid id)
        {
            try
            {
                var categoryDto = await _categoryService.GetCategoryDetailsAsync(id);
                if (categoryDto == null || categoryDto.Products == null || categoryDto.ParentCategoryId != null)
                    return PartialView("_CategoryProductsPartial", new List<ProductDto>());

                return PartialView("_CategoryProductsPartial", categoryDto.Products);
            }
            catch
            {
                return StatusCode(500, "An error occurred while loading products.");
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetSubcategoriesByMainCategory(Guid id)
        {
            try
            {
                var categoryDto = await _categoryService.GetSubCategoriesByCategoryId(id);
                if (categoryDto == null || categoryDto.SubCategories == null)
                    return PartialView("_SubcategoriesPartial", new List<CategoryDto>());

                return PartialView("_SubcategoriesPartial", categoryDto.SubCategories);
            }
            catch
            {
                return StatusCode(500, "An error occurred while loading subcategories.");
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetMainCategoriesAjax()
        {
            try
            {
                var result = await _categoryService.GetFilteredCategoriesAsync("Main", "", "");
                
                var categories = result.Select(c => new
                {
                    c.Id,
                    c.Name,
                    c.Slug
                }).OrderBy(c => c.Name);

                return Ok(categories);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error retrieving main categories", error = ex.Message });
            }
        }
    }
}