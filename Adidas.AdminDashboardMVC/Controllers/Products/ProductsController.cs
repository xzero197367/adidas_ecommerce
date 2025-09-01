using Adidas.Application.Contracts.RepositoriesContracts.Main;
using Adidas.Application.Contracts.RepositoriesContracts.Separator;
using Adidas.Application.Contracts.ServicesContracts.Main;
using Adidas.DTOs.Main.Product_DTOs;
using Adidas.DTOs.Main.ProductDTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Models.People;

namespace Adidas.AdminDashboardMVC.Controllers.Products
{
    [Authorize(Policy = "EmployeeOrAdmin")]

    public class ProductsController : Controller
    {
        private readonly IProductService _productService;
        private readonly ICategoryRepository _categoryService;
        private readonly IBrandRepository _brandService;
        private readonly IProductVariantService _productVariantService;

        public ProductsController(
            IProductService productService,
            ICategoryRepository categoryService,
            IBrandRepository brandService,
            IProductVariantService productVariantService)
        {
            _productService = productService;
            _categoryService = categoryService;
            _brandService = brandService;
            _productVariantService = productVariantService;
        }

        public async Task<IActionResult> Index(ProductFilterDto filter)
        {
            var categories = await _categoryService.GetAll().ToListAsync();
            ViewBag.Categories = categories
                .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name })
                .ToList();

            var brands = await _brandService.GetAll().ToListAsync();
            ViewBag.Brands = brands
                .Select(b => new SelectListItem { Value = b.Id.ToString(), Text = b.Name })
                .ToList();

            ViewBag.Genders = Enum.GetValues(typeof(Gender))
                .Cast<Gender>()
                .Select(g => new SelectListItem
                {
                    Value = ((int)g).ToString(),
                    Text = g.ToString()
                }).ToList();

            var pagedResult = await _productService.GetProductsFilteredByCategoryBrandGenderAsync(filter);
            var itemsList = pagedResult.Items.ToList(); 

            ViewBag.Filter = filter; 

            return View("Index", pagedResult);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await PopulateDropdownsAsync();
            return View(new ProductCreateDto());
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductCreateDto model)
        {
            if (!ModelState.IsValid)
            {
                await PopulateDropdownsAsync();
                return View(model);
            }

            await _productService.CreateAsync(model);
            return RedirectToAction(nameof(Index));
        }


        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
        {
            var result = await _productService.GetByIdAsync(id);
            if (result.IsSuccess == false) return NotFound();
            var product = result.Data;

            await PopulateDropdownsAsync();
            var updateDto = new ProductUpdateDto
            {
                Id = product.Id,
                Name = product.Name,
                Price = product.Price,
                SalePrice = product.SalePrice,
                CategoryId = product.CategoryId,
                BrandId = product.BrandId,
                GenderTarget = product.GenderTarget,
                Description = product.Description,
                InStock = product.InStock
            };

            return View(updateDto);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ProductUpdateDto model)
        {
            if (!ModelState.IsValid)
            {
                await PopulateDropdownsAsync();
                return View(model);
            }

            await _productService.UpdateAsync(model);
            return RedirectToAction(nameof(Index));
        }



        private async Task PopulateDropdownsAsync()
        {
            var categories = await _categoryService.GetAll().Where(p=>p.ParentCategoryId!=null).ToListAsync();
            ViewBag.Categories = categories.Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = c.Name
            }).ToList();

            var brands = await _brandService.GetAll().ToListAsync();
            ViewBag.Brands = brands.Select(b => new SelectListItem
            {
                Value = b.Id.ToString(),
                Text = b.Name
            }).ToList();

            ViewBag.Genders = Enum.GetValues(typeof(Gender))
                .Cast<Gender>()
                .Select(g => new SelectListItem
                {
                    Value = ((int)g).ToString(),
                    Text = g.ToString()
                }).ToList();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id)
        {
            var result = await _productService.GetByIdAsync(id);
            if (!result.IsSuccess)
            {
                TempData["Error"] = "Product not found. " + result.ErrorMessage;
                return RedirectToAction(nameof(Index));
            }

            var product = result.Data;

            if (product.Variants != null && product.Variants.Any())
            {
                TempData["Error"] = $"Cannot delete product because it has {product.Variants.Count()} variant(s).";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                await _productService.DeleteAsync(id);
                TempData["Success"] = "Product deleted successfully.";
            }
            catch
            {
                TempData["Error"] = "An error occurred while deleting the product.";
            }

            return RedirectToAction(nameof(Index));
        }


        public async Task<IActionResult> Details(Guid id, string? sku)
        {
            var product = await _productService.GetProductWithVariantsAsync(id);
            if (product == null) return NotFound();

            if (!string.IsNullOrEmpty(sku))
            {
                product.Variants = product.Variants
                    .Where(v => v.Sku.Contains(sku, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteVariant(Guid id, Guid productId)
        {
            var variant = await _productService.GetVariantByIdAsync(id);
            if (variant == null)
            {
                TempData["Error"] = "Variant not found.";
                return RedirectToAction("Details", new { id = productId });
            }

            await _productService.DeleteVariantAsync(id);

            TempData["Success"] = "Variant deleted successfully.";
            return RedirectToAction("Details", new { id = productId });
        }
    }
}