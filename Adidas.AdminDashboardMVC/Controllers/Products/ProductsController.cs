using Adidas.Application.Contracts.RepositoriesContracts.Main;
using Adidas.Application.Contracts.RepositoriesContracts.Separator;
using Adidas.Application.Contracts.ServicesContracts.Main;
using Adidas.DTOs.Main.Product_DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Models.People;

namespace Adidas.AdminDashboardMVC.Controllers.Products
{
    public class ProductsController : Controller
    {
        private readonly IProductService _productService;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IBrandRepository _brandRepository;
        private readonly IProductVariantRepository _productVariantRepository;

        public ProductsController(
            IProductService productService,
            ICategoryRepository categoryRepository,
            IBrandRepository brandRepository,
            IProductVariantRepository productVariantRepository)
        {
            _productService = productService;
            _categoryRepository = categoryRepository;
            _brandRepository = brandRepository;
            _productVariantRepository = productVariantRepository;
        }

        public async Task<IActionResult> Index(ProductFilterDto filter)
        {

            var categories = await _categoryRepository.GetAllAsync();
            ViewBag.Categories = categories
                .Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.Name
                }).ToList();

            var brands = await _brandRepository.GetAllAsync();
            ViewBag.Brands = brands
                .Select(b => new SelectListItem
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

            var pagedResult = await _productService.GetProductsFilteredByCategoryBrandGenderAsync(filter);

            return View(pagedResult);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await PopulateDropdownsAsync();
            return View(new CreateProductDto());
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateProductDto model)
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
            var product = await _productService.GetByIdAsync(id);
            if (product == null) return NotFound();

            await PopulateDropdownsAsync();
            var updateDto = new UpdateProductDto
            {
                ID = product.Id,
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
        public async Task<IActionResult> Edit(UpdateProductDto model)
        {
            if (!ModelState.IsValid)
            {
                await PopulateDropdownsAsync();
                return View(model);
            }

            await _productService.UpdateAsync(model.ID, model);
            return RedirectToAction(nameof(Index));
        }


        private async Task PopulateDropdownsAsync()
        {
            var categories = await _categoryRepository.GetAllAsync();
            ViewBag.Categories = categories.Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = c.Name
            }).ToList();

            var brands = await _brandRepository.GetAllAsync();
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
            var product = await _productService.GetByIdAsync(id);
            if (product == null)
            {
                TempData["Error"] = "Product not found.";
                return RedirectToAction(nameof(Index));
            }

            if (product.Variants != null && product.Variants.Any())
            {
                return RedirectToAction(nameof(Index));
            }

            await _productService.DeleteAsync(id);
            TempData["Success"] = "Product deleted successfully.";
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
