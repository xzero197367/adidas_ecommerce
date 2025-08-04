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

                // ✅ Create directory if it doesn't exist
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

            var success = await _categoryService.CreateAsync(model);

            if (!success)
            {
                ModelState.AddModelError("", "An error occurred while creating the category.");
                await PopulateParentCategoriesDropdown();
                return View(model);
            }

            return RedirectToAction("Index");
        }




        private async Task PopulateParentCategoriesDropdown()
        {
            var parentCategories = await _categoryService.GetMainCategoriesAsync(); // Use method that fetches only main/active ones if needed

            ViewBag.ParentCategories = new SelectList(parentCategories, "Id", "Name");
        }
    }


}

