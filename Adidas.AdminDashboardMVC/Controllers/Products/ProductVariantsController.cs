using Adidas.Application.Contracts.RepositoriesContracts.Separator;
using Adidas.Application.Contracts.ServicesContracts.Main;
using Adidas.Application.Contracts.ServicesContracts.Separator;
using Adidas.DTOs.Main.Product_Variant_DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Models.People;
using System;
using System.Threading.Tasks;
using Adidas.DTOs.Main.ProductDTOs;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace Adidas.AdminDashboardMVC.Controllers.Products
{
    [Authorize(Policy = "EmployeeOrAdmin")]

    public class ProductVariantsController : Controller
    {
        private readonly IProductVariantService _productVariantService;
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;
        private readonly IBrandService _brandService;

        public ProductVariantsController(
            IProductVariantService productVariantService,
            IProductService productService,
            ICategoryService categoryService,
            IBrandService brandService, IBrandRepository brandRepository)
        {
            _productVariantService = productVariantService;
            _productService = productService;
            _categoryService = categoryService;
            _brandService = brandService;
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
                var result = await _productVariantService.GetAllAsync();
                variants = result.Data;
            }

            return View(variants);
        }


        // GET: Create Form
        public async Task<IActionResult> Create(Guid productId)
        {
            await PopulateDropdownsAsync(productId);
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([FromForm] ProductVariantCreateDto createDto)
        {
            if (createDto.ImageFile == null)
            {
                ModelState.AddModelError("ImageFile", "Image is Required");
            }
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                TempData["Error"] = string.Join(" | ", errors);
                await PopulateDropdownsAsync();
                return View(createDto);
            }

            try
            {
                var createdVariant = await _productVariantService.CreateAsync(createDto);
                TempData["Success"] = "Product variant created successfully!";
                return RedirectToAction("Details", "Products", new { id = createDto.ProductId });
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError("", ex.Message);
                await PopulateDropdownsAsync(createDto.ProductId);
                return View(createDto);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var result = await _productVariantService.DeleteAsync(id);

                if (result.IsSuccess)
                    TempData["Success"] = "Product variant deleted successfully.";
                else
                    TempData["Error"] = result.ErrorMessage ?? "Unknown error occurred.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error deleting variant: {ex.Message}";
            }

            // 👇 get the page that called me
            var referer = Request.Headers["Referer"].ToString();

            if (!string.IsNullOrEmpty(referer))
                return Redirect(referer); // go back to previous page
            else
                return RedirectToAction(nameof(Index)); // fallback
        }



        private async Task PopulateDropdownsAsync(Guid? selectedProductId = null)
        {
            var results = await _productService.GetAllAsync();
            var products = results.Data;
            ViewBag.Products = new SelectList(products, "Id", "Name", selectedProductId);

            var categories = await _categoryService.GetAllAsync();
            ViewBag.Categories = new SelectList(categories.Data, "Id", "Name");

            var brands = await _brandService.GetAllAsync();
            ViewBag.Brands = new SelectList(brands.Data, "Id", "Name");

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
            var result = await _productVariantService.GetByIdAsync(id);
            if (result.IsSuccess == false)
            {
                TempData["Error"] = result.ErrorMessage;
                // return RedirectToAction(nameof(Index));
                return NotFound();
            }

            var variant = result.Data;

            var updateDto = new ProductVariantUpdateDto
            {
                Id = variant.Id,
                ProductId = variant.ProductId,
                Color = variant.Color,
                Size = variant.Size,
                StockQuantity = variant.StockQuantity,
                PriceAdjustment = variant.PriceAdjustment,
                ColorHex = variant.ColorHex,
                SortOrder = variant.SortOrder,
                ImageUrl = variant.ImageUrl
            };

            var productsResult = await _productService.GetAllAsync();
            var products = productsResult.Data;
            ViewBag.Products = products.Select(p => new SelectListItem
            {
                Value = p.Id.ToString(),
                Text = p.Name
            }).ToList();

            return View(updateDto);
        }


        // POST: /ProductVariants/Edit/{id}
        // Add this debugging code at the start of your Edit POST method
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit([FromForm] ProductVariantUpdateDto model)
        {
            // Remove the required image validation for edit - it should be optional
            // Only validate image if a new one is being uploaded
            if (model.ImageFile != null && model.ImageFile.Length == 0)
            {
                ModelState.Remove("ImageFile");
            }

            // DEBUG: Log what we received
            var logger = HttpContext.RequestServices.GetService<ILogger<ProductVariantsController>>();
            logger?.LogInformation("=== EDIT POST DEBUG ===");
            logger?.LogInformation("Route ID: {Id}", model.Id);
            logger?.LogInformation("Model ID: {ModelId}", model.Id);
            logger?.LogInformation("ProductId: {ProductId}", model.ProductId);
            logger?.LogInformation("Color: {Color}", model.Color);
            logger?.LogInformation("ColorHex: {ColorHex}", model.ColorHex);
            logger?.LogInformation("Size: {Size}", model.Size);
            logger?.LogInformation("StockQuantity: {StockQuantity}", model.StockQuantity);
            logger?.LogInformation("PriceAdjustment: {PriceAdjustment}", model.PriceAdjustment);
            logger?.LogInformation("ImageUrl: {ImageUrl}", model.ImageUrl);

            // DEBUG: Check the ImageFile
            if (model.ImageFile != null && model.ImageFile.Length > 0)
            {
                logger?.LogInformation("✓ ImageFile received:");
                logger?.LogInformation("  - Name: {FileName}", model.ImageFile.FileName);
                logger?.LogInformation("  - Size: {FileSize} bytes", model.ImageFile.Length);
                logger?.LogInformation("  - ContentType: {ContentType}", model.ImageFile.ContentType);
            }
            else
            {
                logger?.LogInformation("ℹ No new ImageFile received - will keep existing image");
            }

            // DEBUG: Check form data directly
            logger?.LogInformation("Form data received:");
            foreach (var key in Request.Form.Keys)
            {
                var value = Request.Form[key];
                logger?.LogInformation("  {Key}: {Value}", key, value);
            }

            // DEBUG: Check files specifically
            logger?.LogInformation("Files received: {FileCount}", Request.Form.Files.Count);
            foreach (var file in Request.Form.Files)
            {
                logger?.LogInformation("  File: {Name} = {FileName} ({Length} bytes)",
                    file.Name, file.FileName, file.Length);
            }

            //if (id != model.Id)
            //    return BadRequest();

            if (!ModelState.IsValid)
            {
                // DEBUG: Log validation errors
                logger?.LogWarning("ModelState is invalid:");
                foreach (var error in ModelState)
                {
                    foreach (var subError in error.Value.Errors)
                    {
                        logger?.LogWarning("  {Key}: {Error}", error.Key, subError.ErrorMessage);
                    }
                }

                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                TempData["Error"] = string.Join(" | ", errors);

                var productResult = await _productService.GetAllAsync();
                var products = productResult.Data;
                ViewBag.Products = products.Select(p => new SelectListItem
                {
                    Value = p.Id.ToString(),
                    Text = p.Name
                }).ToList();

                return View(model);
            }

            try
            {
                var result = await _productVariantService.UpdateAsync(model);

                if (!result.IsSuccess)
                {
                    logger?.LogError("Service update failed: {Error}", result.ErrorMessage);
                    ModelState.AddModelError("", result.ErrorMessage);

                    var productResult = await _productService.GetAllAsync();
                    var products = productResult.Data;
                    ViewBag.Products = products.Select(p => new SelectListItem
                    {
                        Value = p.Id.ToString(),
                        Text = p.Name
                    }).ToList();

                    return View(model);
                }

                logger?.LogInformation("✓ Product variant updated successfully");
                TempData["Success"] = "Product variant updated successfully!";
                return RedirectToAction("Details", "Products", new { id = model.ProductId });
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "Exception during update");
                ModelState.AddModelError("", $"Error updating product variant: {ex.Message}");

                var productResult = await _productService.GetAllAsync();
                var products = productResult.Data;
                ViewBag.Products = products.Select(p => new SelectListItem
                {
                    Value = p.Id.ToString(),
                    Text = p.Name
                }).ToList();

                return View(model);
            }
        }

        // api call to get all variants
        [HttpGet("variants")]
        public async Task<IActionResult> GetVariants(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 12,
            [FromQuery] string? search = null,
            [FromQuery] Guid? categoryId = null,
            [FromQuery] Guid? brandId = null,
            [FromQuery] Gender? gender = null,
            [FromQuery] bool inStockOnly = true)
        {
            try
            {
                var filter = new ProductFilterDto
                {
                    PageNumber = page,
                    PageSize = pageSize,
                    SearchTerm = search,
                    CategoryId = categoryId,
                    BrandId = brandId,
                    Gender = gender,
                    InStock = inStockOnly
                };

                var result = await _productService.GetProductsWithFiltersAsync(filter);

                if (!result.IsSuccess)
                {
                    return BadRequest(new { message = result.ErrorMessage });
                }

                // Transform products to variants for the response
                var variants = result.Data.Items
                    .SelectMany(p => p.Variants.Select(v => new
                    {
                        Id = v.Id,
                        Sku = v.Sku,
                        Color = v.Color,
                        Size = v.Size,
                        StockQuantity = v.StockQuantity,
                        PriceAdjustment = v.PriceAdjustment,
                        ImageUrl = v.Images.FirstOrDefault()?.ImageUrl ?? p.Images.FirstOrDefault()?.ImageUrl,
                        Product = new
                        {
                            Id = p.Id,
                            Name = p.Name,
                            Price = p.Price,
                            SalePrice = p.SalePrice,
                            DisplayPrice = p.DisplayPrice,
                            CategoryName = p.CategoryName,
                            BrandName = p.BrandName
                        }
                    }))
                    .Where(v => !inStockOnly || v.StockQuantity > 0)
                    .ToList();

                var response = new
                {
                    Items = variants,
                    TotalCount = variants.Count,
                    PageNumber = page,
                    PageSize = pageSize,
                    TotalPages = (int)Math.Ceiling((double)variants.Count / pageSize)
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error retrieving variants", error = ex.Message });
            }
        }

        [HttpGet("variants/{id}")]
        public async Task<IActionResult> GetVariant(Guid id)
        {
            try
            {
                var variant = await _productVariantService.GetByIdAsync(id);
                if (!variant.IsSuccess || variant.Data == null)
                {
                    return NotFound(new { message = "Variant not found" });
                }

                return Ok(variant.Data);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error retrieving variant", error = ex.Message });
            }
        }
    }
}