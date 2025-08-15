using Microsoft.AspNetCore.Mvc;
using Adidas.Application.Contracts.ServicesContracts.Main;
using Adidas.DTOs.Main.Product_Variant_DTOs;
using Adidas.DTOs.Main.ProductImageDTOs;

namespace Adidas.AdminDashboardMVC.Controllers.Main;

[Route("Admin/[controller]")]
public class ProductVariant1Controller : Controller
{
    private readonly IProductVariantService _productVariantService;
    private readonly IProductService _productService;
    private readonly IProductImageService _productImageService;
    private readonly ILogger<ProductVariant1Controller> _logger;

    public ProductVariant1Controller(
        IProductVariantService productVariantService,
        IProductService productService,
        IProductImageService productImageService,
        ILogger<ProductVariant1Controller> logger)
    {
        _productVariantService = productVariantService;
        _productService = productService;
        _productImageService = productImageService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> Index(Guid? productId = null)
    {
        try
        {
            var result = await _productVariantService.GetAllAsync();
            if (result.IsSuccess)
            {
                var variants = result.Data;
                if (productId.HasValue)
                {
                    variants = variants.Where(v => v.ProductId == productId.Value);
                }

                ViewBag.ProductId = productId;
                return View(variants);
            }

            TempData["ErrorMessage"] = result.ErrorMessage;
            return View(new List<ProductVariantDto>());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while loading product variants");
            TempData["ErrorMessage"] = "An error occurred while loading product variants.";
            return View(new List<ProductVariantDto>());
        }
    }

    [HttpGet("Details/{id}")]
    public async Task<IActionResult> Details(Guid id)
    {
        var result = await _productVariantService.GetByIdAsync(id);
        if (!result.IsSuccess)
        {
            TempData["ErrorMessage"] = result.ErrorMessage;
            return RedirectToAction(nameof(Index));
        }

        // Get variant images
        var imagesResult = await _productImageService.GetImagesByVariantIdAsync(id);
        ViewBag.VariantImages = imagesResult.IsSuccess ? imagesResult.Data : new List<ProductImageDto>();

        return View(result.Data);
    }

    [HttpGet("Create")]
    public async Task<IActionResult> Create(Guid? productId = null)
    {
        var model = new ProductVariantCreateDto();
        if (productId.HasValue)
        {
            model.ProductId = productId.Value;

            var productResult = await _productService.GetByIdAsync(productId.Value);
            if (productResult.IsSuccess)
            {
                ViewBag.ProductName = productResult.Data.Name;
            }
        }

        await PopulateProductsList();
        return View(model);
    }

    [HttpPost("Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ProductVariantCreateDto model)
    {
        if (ModelState.IsValid)
        {
            var result = await _productVariantService.CreateAsync(model);
            if (result.IsSuccess)
            {
                TempData["SuccessMessage"] = "Product variant created successfully.";
                return RedirectToAction("Details", new { id = result.Data.Id });
            }

            TempData["ErrorMessage"] = result.ErrorMessage;
        }

        await PopulateProductsList();

        if (model.ProductId != Guid.Empty)
        {
            var productResult = await _productService.GetByIdAsync(model.ProductId);
            if (productResult.IsSuccess)
            {
                ViewBag.ProductName = productResult.Data.Name;
            }
        }

        return View(model);
    }

    [HttpGet("Edit/{id}")]
    public async Task<IActionResult> Edit(Guid id)
    {
        var result = await _productVariantService.GetByIdAsync(id);
        if (!result.IsSuccess)
        {
            TempData["ErrorMessage"] = result.ErrorMessage;
            return RedirectToAction(nameof(Index));
        }

        var updateDto = new ProductVariantUpdateDto
        {
            Id = result.Data.Id,
            ProductId = result.Data.ProductId,
            Color = result.Data.Color,
            Size = result.Data.Size,
            StockQuantity = result.Data.StockQuantity,
            PriceAdjustment = result.Data.PriceAdjustment,
            ColorHex = result.Data.ColorHex,
            SortOrder = result.Data.SortOrder,
            ImageUrl = result.Data.Images?.FirstOrDefault()?.ImageUrl
        };

        await PopulateProductsList();

        var productResult = await _productService.GetByIdAsync(result.Data.ProductId);
        if (productResult.IsSuccess)
        {
            ViewBag.ProductName = productResult.Data.Name;
        }

        return View(updateDto);
    }

    [HttpPost("Edit/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, ProductVariantUpdateDto model)
    {
        if (id != model.Id)
        {
            return BadRequest();
        }

        if (ModelState.IsValid)
        {
            model.Id = id;
            var result = await _productVariantService.UpdateAsync(model);
            if (result.IsSuccess)
            {
                TempData["SuccessMessage"] = "Product variant updated successfully.";
                return RedirectToAction("Details", new { id = result.Data?.Id });
            }

            TempData["ErrorMessage"] = result.ErrorMessage;
        }

        await PopulateProductsList();

        var productResult = await _productService.GetByIdAsync(model.ProductId);
        if (productResult.IsSuccess)
        {
            ViewBag.ProductName = productResult.Data?.Name;
        }

        return View(model);
    }

    [HttpPost("Delete/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _productVariantService.DeleteAsync(id);
        if (result.IsSuccess)
        {
            TempData["SuccessMessage"] = "Product variant deleted successfully.";
        }
        else
        {
            TempData["ErrorMessage"] = result.ErrorMessage;
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost("UpdateStock/{id}")]
    public async Task<IActionResult> UpdateStock(Guid id, int stockQuantity)
    {
        try
        {
            var variantResult = await _productVariantService.GetByIdAsync(id);
            if (!variantResult.IsSuccess)
            {
                return Json(new { success = false, message = "Variant not found." });
            }

            var updateDto = new ProductVariantUpdateDto
            {
                Id = id,
                ProductId = variantResult.Data.ProductId,
                Color = variantResult.Data.Color,
                Size = variantResult.Data.Size,
                StockQuantity = stockQuantity,
                PriceAdjustment = variantResult.Data.PriceAdjustment
            };

            updateDto.Id = id;
            var result = await _productVariantService.UpdateAsync(updateDto);

            return Json(new
            {
                success = result.IsSuccess,
                message = result.IsSuccess ? "Stock updated successfully." : result.ErrorMessage
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating stock for variant {VariantId}", id);
            return Json(new { success = false, message = "An error occurred while updating stock." });
        }
    }

    [HttpPost("AddImage/{id}")]
    public async Task<IActionResult> AddImage(Guid id, IFormFile imageFile)
    {
        if (imageFile == null || imageFile.Length == 0)
        {
            return Json(new { success = false, message = "Please select a valid image file." });
        }

        var result = await _productVariantService.AddImageAsync(id, imageFile);

        return Json(new
        {
            success = result,
            message = result ? "Image added successfully." : "Failed to add image."
        });
    }

    [HttpGet("GetByProduct/{productId}")]
    public async Task<IActionResult> GetByProduct(Guid productId)
    {
        var result = await _productVariantService.GetAllAsync();
        if (result.IsSuccess)
        {
            var variants = result.Data.Where(v => v.ProductId == productId);
            return Json(variants.Select(v => new
            {
                id = v.Id,
                sku = v.Sku,
                color = v.Color,
                size = v.Size,
                stockQuantity = v.StockQuantity,
                priceAdjustment = v.PriceAdjustment,
                isActive = v.IsActive
            }));
        }

        return Json(new List<object>());
    }

    [HttpGet("GetBySku/{sku}")]
    public async Task<IActionResult> GetBySku(string sku)
    {
        var result = await _productVariantService.GetBySkuAsync(sku);
        if (result != null)
        {
            return Json(new
            {
                id = result.Id,
                productId = result.ProductId,
                sku = result.Sku,
                color = result.Color,
                size = result.Size,
                stockQuantity = result.StockQuantity,
                priceAdjustment = result.PriceAdjustment
            });
        }

        return Json(null);
    }

    [HttpGet("GetStockStatus")]
    public async Task<IActionResult> GetStockStatus(int threshold = 10)
    {
        try
        {
            var result = await _productVariantService.GetAllAsync();
            if (result.IsSuccess)
            {
                var lowStockVariants = result.Data
                    .Where(v => v.StockQuantity <= threshold)
                    .Select(v => new
                    {
                        id = v.Id,
                        productName = v.Product?.Name,
                        sku = v.Sku,
                        color = v.Color,
                        size = v.Size,
                        stockQuantity = v.StockQuantity,
                        status = v.StockQuantity == 0 ? "Out of Stock" :
                            v.StockQuantity <= 5 ? "Critical" : "Low"
                    });

                return Json(lowStockVariants);
            }

            return Json(new List<object>());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting stock status");
            return Json(new List<object>());
        }
    }

    private async Task PopulateProductsList()
    {
        var result = await _productService.GetAllAsync();
        if (result.IsSuccess)
        {
            ViewBag.Products = result.Data.Select(p => new
            {
                Id = p.Id,
                Name = p.Name,
                Sku = p.Sku
            });
        }
        else
        {
            ViewBag.Products = new List<object>();
        }
    }
}