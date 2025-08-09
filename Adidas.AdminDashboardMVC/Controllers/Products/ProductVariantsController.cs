using Adidas.Application.Contracts.RepositoriesContracts.Separator;
using Adidas.Application.Contracts.ServicesContracts.Main;
using Adidas.Application.Contracts.ServicesContracts.Separator;
using Adidas.DTOs.Main.Product_Variant_DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Models.People;
using System;
using System.Threading.Tasks;

namespace Adidas.AdminDashboardMVC.Controllers.Products
{
    public class ProductVariantsController : Controller
    {
        private readonly IProductVariantService _productVariantService;
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;
        private readonly IBrandService _brandService;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IBrandRepository _brandRepository;

        public ProductVariantsController(
            IProductVariantService productVariantService,
            IProductService productService,
            ICategoryService categoryService,
            IBrandService brandService, ICategoryRepository categoryRepository, IBrandRepository brandRepository)
        {

            _productVariantService = productVariantService;
            _productService = productService;
            _categoryService = categoryService;
            _brandService = brandService;
            _categoryRepository = categoryRepository;
            _brandRepository = brandRepository;
        }
        // GET: /ProductVariants?searchSku=XXXX
        public async Task<IActionResult> Index(string? searchSku)
        {
            IEnumerable<ProductVariantDto> variants;

            if (!string.IsNullOrWhiteSpace(searchSku))
            {
                var variant = await _productVariantService.GetBySkuAsync(searchSku.Trim());
                if (variant != null)
                {
                    variants = new List<ProductVariantDto> { variant };
                }
                else
                {
                    TempData["Error"] = $"No variant found with SKU: {searchSku}";
                    variants = new List<ProductVariantDto>();
                }
            }
            else
            {
                variants = await _productVariantService.GetAllAsync();
            }

            return View(variants);
        }


        // GET: Create Form
        public async Task<IActionResult> Create()
        {
            await PopulateDropdownsAsync();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([FromForm] CreateProductVariantDto createDto)
        {
            if (!ModelState.IsValid)
            {
                await PopulateDropdownsAsync();
                return View(createDto);
            }

            try
            {
                var createdVariant = await _productVariantService.CreateAsync(createDto);
                TempData["Success"] = "Product variant created successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError("", ex.Message);
                await PopulateDropdownsAsync();
                return View(createDto);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                await _productVariantService.DeleteAsync(id);
                TempData["Success"] = "Product variant deleted successfully.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error deleting variant: {ex.Message}";
            }
            return RedirectToAction(nameof(Index));
        }




        private async Task PopulateDropdownsAsync()
        {
            var products = await _productService.GetAllAsync();
            ViewBag.Products = products.Select(p => new SelectListItem
            {
                Value = p.Id.ToString(),
                Text = p.Name
            }).ToList();

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
        // GET: /ProductVariants/Edit/{id}
        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
        {
            var variant = await _productVariantService.GetByIdAsync(id);
            if (variant == null)
                return NotFound();

            var updateDto = new UpdateProductVariantDto
            {
                Id = variant.Id,
                ProductId = variant.ProductId,
                Color = variant.Color,
                Size = variant.Size,
                StockQuantity = variant.StockQuantity,
                PriceAdjustment = variant.PriceAdjustment,
                ColorHex = variant.ColorHex,
                SortOrder = variant.SortOrder,
                ImageUrl = variant.Images.FirstOrDefault()?.ImageUrl
            };

            var products = await _productService.GetAllAsync();
            ViewBag.Products = products.Select(p => new SelectListItem
            {
                Value = p.Id.ToString(),
                Text = p.Name
            }).ToList();

            return View(updateDto);
        }


        // POST: /ProductVariants/Edit/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, UpdateProductVariantDto model)
        {
            if (id != model.Id)
                return BadRequest();

            if (!ModelState.IsValid)
            {
                var products = await _productService.GetAllAsync();
                ViewBag.Products = products.Select(p => new SelectListItem
                {
                    Value = p.Id.ToString(),
                    Text = p.Name
                }).ToList();
                return View(model);
            }

            try
            {
                await _productVariantService.UpdateAsync(id, model);
                TempData["Success"] = "Product variant updated successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error updating product variant: {ex.Message}");
                var products = await _productService.GetAllAsync();
                ViewBag.Products = products.Select(p => new SelectListItem
                {
                    Value = p.Id.ToString(),
                    Text = p.Name
                }).ToList();
                return View(model);
            }
        }

    }
}