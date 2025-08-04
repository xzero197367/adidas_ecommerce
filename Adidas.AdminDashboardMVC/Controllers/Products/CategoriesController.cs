using Adidas.Application.Contracts.ServicesContracts.Separator;
using Adidas.DTOs.Separator.Category_DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Adidas.AdminDashboardMVC.Controllers.Products
{
    public class CategoriesController : Controller
    {
        private readonly ICategoryService _categoryService;
        private readonly IWebHostEnvironment _webHostEnvironment;


        public CategoriesController(ICategoryService categoryService, IWebHostEnvironment webHostEnvironment)
        {
            _categoryService = categoryService;
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task<IActionResult> Index()
        {
            var mainCategories = await _categoryService.GetMainCategoriesAsync();
            return View(mainCategories);
        }


        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await PopulateParentCategoriesDropdown();
            return View();
        }

        // POST: /Category/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateCategoryDto model, IFormFile ImageFile)
        {
            if (!ModelState.IsValid)
            {
                await PopulateParentCategoriesDropdown();
                return View(model);
            }

            if (ImageFile != null && ImageFile.Length > 0)
            {
                if (ImageFile.Length > 5 * 1024 * 1024)
                {
                    ModelState.AddModelError("ImageUrl", "Image size should not exceed 5MB.");
                    await PopulateParentCategoriesDropdown();
                    return View(model);
                }

                var fileName = Guid.NewGuid() + Path.GetExtension(ImageFile.FileName);
                var relativePath = Path.Combine("uploads", "categories", fileName);
                var absolutePath = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "categories");

                if (!Directory.Exists(absolutePath))
                {
                    Directory.CreateDirectory(absolutePath);
                }

                var filePath = Path.Combine(absolutePath, fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await ImageFile.CopyToAsync(stream);
                }

                model.ImageUrl = "/" + relativePath.Replace("\\", "/");
            }

            // ✅ Get Result instead of just bool
            var result = await _categoryService.CreateAsync(model);

            if (!result.IsSuccess)
            {
                ModelState.AddModelError(string.Empty, result.Error); // Show specific error (e.g., slug exists)
                TempData["Error"] = result.Error;

                await PopulateParentCategoriesDropdown();
                return View(model);
            }

            TempData["Success"] = "Category created successfully!";
            return RedirectToAction("Index");
        }


        // GET: /Category/Edit/{id}
        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
        {
            var category = await _categoryService.GetByIdAsync(id);
            if (category == null)
                return NotFound();

            var model = new UpdateCategoryDto
            {
                Name = category.Name,
                Slug = category.Slug,
                Description = category.Description,
                ImageUrl = category.ImageUrl,
                SortOrder = category.SortOrder,
                ParentCategoryId = category.ParentCategoryId
            };

            ViewBag.CategoryId = id;
            ViewBag.CurrentImageUrl = category.ImageUrl;
            await PopulateParentCategoriesDropdown();
            return View(model);
        }

        // POST: /Category/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit( UpdateCategoryDto model, IFormFile ImageFile)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.CategoryId = model.Id;
                await PopulateParentCategoriesDropdown();
                return View(model);
            }

            if (ImageFile != null && ImageFile.Length > 0)
            {
                if (ImageFile.Length > 5 * 1024 * 1024)
                {
                    ModelState.AddModelError("ImageUrl", "Image size should not exceed 5MB.");
                    ViewBag.CategoryId = model.Id;
                    await PopulateParentCategoriesDropdown();
                    return View(model);
                }

                var fileName = Guid.NewGuid() + Path.GetExtension(ImageFile.FileName);
                var relativePath = Path.Combine("uploads", "categories", fileName);
                var absolutePath = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "categories");

                if (!Directory.Exists(absolutePath))
                {
                    Directory.CreateDirectory(absolutePath);
                }

                var filePath = Path.Combine(absolutePath, fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await ImageFile.CopyToAsync(stream);
                }

                model.ImageUrl = "/" + relativePath.Replace("\\", "/");
            }

            var result = await _categoryService.UpdateAsync(model);
            if (!result.IsSuccess)
            {
                ModelState.AddModelError(string.Empty, result.Error);
                TempData["Error"] = result.Error;
                ViewBag.CategoryId = model.Id;
                await PopulateParentCategoriesDropdown();
                return View(model);
            }

            TempData["Success"] = "Category updated successfully!";
            return RedirectToAction("Index");
        }



        private async Task PopulateParentCategoriesDropdown()
        {
            var parentCategories = await _categoryService.GetMainCategoriesAsync(); // Use method that fetches only main/active ones if needed

            ViewBag.ParentCategories = new SelectList(parentCategories, "Id", "Name");
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
                TempData["Success"] = "Category deleted successfully!";
            }

            return RedirectToAction("Index");
        }

    }



}

