using Adidas.Application.Contracts.RepositoriesContracts.Main;
using Adidas.Application.Contracts.RepositoriesContracts.Separator;
using Adidas.Application.Contracts.ServicesContracts.Main;
using Adidas.Application.Contracts.ServicesContracts.Separator;
using Adidas.DTOs.Common_DTOs;
using Adidas.DTOs.Main.Product_DTOs;
using Adidas.DTOs.Main.ProductDTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
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
        private readonly ICategoryService _categoryService;
        private readonly IBrandRepository _brandService;
        private readonly IProductVariantService _productVariantService;

        public ProductsController(
            IProductService productService,
            ICategoryService categoryService,
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
            // Validate the filter
            if (!ModelState.IsValid)
            {
                // Repopulate ViewBags for dropdowns even if validation fails
                var categories = await _categoryService.GetFilteredCategoriesAsync("Sub", "", "");
                ViewBag.Categories = categories
                    .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name })
                    .ToList();

                var brands = await _brandService.GetAll().ToListAsync();
                // ViewBag.Brands = brands
                //     .Select(b => new SelectListItem { Value = b.Id.ToString(), Text = b.Name })
                //     .ToList();

                ViewBag.Genders = Enum.GetValues(typeof(Gender))
                    .Cast<Gender>()
                    .Select(g => new SelectListItem
                    {
                        Value = ((int)g).ToString(),
                        Text = g.ToString()
                    }).ToList();

                // Keep the invalid filter so user input is preserved
                ViewBag.Filter = filter;

                // Return the view with empty results or previous page
                return View("Index", new PagedResultDto<ProductDto>());
            }

            // Populate dropdowns
            var categories2 = await _categoryService.GetFilteredCategoriesAsync("Sub", "", "");
            ViewBag.Categories = categories2
                .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name })
                .ToList();

            var brands2 = await _brandService.GetAll().ToListAsync();
            // ViewBag.Brands = brands2
            //     .Select(b => new SelectListItem { Value = b.Id.ToString(), Text = b.Name })
            //     .ToList();

            ViewBag.Genders = Enum.GetValues(typeof(Gender))
                .Cast<Gender>()
                .Select(g => new SelectListItem
                {
                    Value = ((int)g).ToString(),
                    Text = g.ToString()
                }).ToList();

            // Fetch filtered products
            var pagedResult = await _productService.GetProductsFilteredByCategoryBrandGenderAsync(filter);

            ViewBag.Filter = filter; // keep user input

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
            // ✅ Check images required
            if (model.Images == null || !model.Images.Any())
            {
                ModelState.AddModelError("Images", "You should add at least one image");
            }
    
            // ✅ Sale price validation
            if (model.SalePrice >= model.Price)
            {
                ModelState.AddModelError(nameof(model.SalePrice), "Sale price must be less than the price");
            }

            // ✅ Return to view if invalid
            if (!ModelState.IsValid)
            {
                await PopulateDropdownsAsync();
                return View(model);
            }

            // ✅ Call service
            var result = await _productService.CreateAsync(model);
            if (!result.IsSuccess)
            {
                ModelState.AddModelError(string.Empty, result.ErrorMessage);
                TempData["Error"] = result.ErrorMessage;

                await PopulateDropdownsAsync();
                return View(model);
            }

            TempData["Success"] = "Product created successfully!";
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
                // Removed BrandId
                GenderTarget = product.GenderTarget,
                Description = product.Description,
                InStock = product.InStock,
                ExistingImages = product.Images.Where(p=>p.VariantId==null).ToList(),
                CurrentImagePath = product.ImageUrl // Use ImageUrl from your ProductDto
            };
            return View(updateDto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ProductUpdateDto model)
        {
            if (model.SalePrice >= model.Price)
            {
                ModelState.AddModelError("SalePrice", "SalePrice must be Less than the price");
            }

            if (!ModelState.IsValid)
            {
                await PopulateDropdownsAsync();
                return View(model);
            }

            var result = await _productService.UpdateAsync(model);
            if (!result.IsSuccess)
            {
                ModelState.AddModelError(string.Empty, result.ErrorMessage);
                TempData["Error"] = result.ErrorMessage;
                await PopulateDropdownsAsync();
                return View(model);
            }

            TempData["Success"] = "Product updated successfully!";
            return RedirectToAction(nameof(Index));
        }
        private async Task PopulateDropdownsAsync()
        {
            var categories = await _categoryService.GetFilteredCategoriesAsync("Sub", "", "");
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