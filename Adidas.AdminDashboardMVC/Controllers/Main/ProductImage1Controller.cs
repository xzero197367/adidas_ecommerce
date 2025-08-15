
using Adidas.Application.Contracts.ServicesContracts.Main;
using Adidas.DTOs.Main.ProductImageDTOs;
using Microsoft.AspNetCore.Mvc;
using File1 = System.IO.File;

namespace Adidas.AdminDashboardMVC.Controllers.Main;

[Route("Admin/[controller]")]
public class ProductImage1Controller : Controller
{
    private readonly IProductImageService _productImageService;
    private readonly IProductService _productService;
    private readonly IProductVariantService _productVariantService;
    private readonly IWebHostEnvironment _webHostEnvironment;

    public ProductImage1Controller(
        IProductImageService productImageService,
        IProductService productService,
        IProductVariantService productVariantService,
        IWebHostEnvironment webHostEnvironment)
    {
        _productImageService = productImageService;
        _productService = productService;
        _productVariantService = productVariantService;
        _webHostEnvironment = webHostEnvironment;
    }

    [HttpGet]
    public async Task<IActionResult> Index(Guid? productId = null, Guid? variantId = null)
    {
        try
        {
            IEnumerable<ProductImageDto>? images;

            if (productId.HasValue)
            {
                var result = await _productImageService.GetImagesByProductIdAsync(productId.Value);
                images = result.IsSuccess ? result.Data : new List<ProductImageDto>();

                var productResult = await _productService.GetByIdAsync(productId.Value);
                if (productResult.IsSuccess)
                {
                    ViewBag.ProductName = productResult.Data?.Name;
                    ViewBag.ProductId = productId;
                }
            }
            else if (variantId.HasValue)
            {
                var result = await _productImageService.GetImagesByVariantIdAsync(variantId.Value);
                images = result.IsSuccess ? result.Data : new List<ProductImageDto>();

                var variantResult = await _productVariantService.GetByIdAsync(variantId.Value);
                if (variantResult.IsSuccess)
                {
                    ViewBag.VariantName =
                        $"{variantResult.Data?.Product?.Name} - {variantResult.Data?.Color} - {variantResult.Data?.Size}";
                    ViewBag.VariantId = variantId;
                    ViewBag.ProductId = variantResult.Data.ProductId;
                }
            }
            else
            {
                var result = await _productImageService.GetAllAsync();
                images = result.IsSuccess ? result.Data : new List<ProductImageDto>();
            }

            return View(images);
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = "An error occurred while loading images.";
            return View(new List<ProductImageDto>());
        }
    }

    [HttpGet("Create")]
    public async Task<IActionResult> Create(Guid? productId = null, Guid? variantId = null)
    {
        var model = new ProductImageCreateDto();

        if (productId.HasValue)
        {
            model.ProductId = productId.Value;

            var productResult = await _productService.GetByIdAsync(productId.Value);
            if (productResult.IsSuccess)
            {
                ViewBag.ProductName = productResult.Data.Name;
            }
        }

        if (variantId.HasValue)
        {
            model.VariantId = variantId.Value;

            var variantResult = await _productVariantService.GetByIdAsync(variantId.Value);
            if (variantResult.IsSuccess)
            {
                model.ProductId = variantResult.Data.ProductId;
                ViewBag.VariantName =
                    $"{variantResult.Data.Product?.Name} - {variantResult.Data.Color} - {variantResult.Data.Size}";
            }
        }

        await PopulateDropdowns();
        return View(model);
    }

    [HttpPost("Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ProductImageCreateDto model, IFormFile? imageFile)
    {
        if (ModelState.IsValid)
        {
            try
            {
                // Handle file upload
                if (imageFile != null && imageFile.Length > 0)
                {
                    var imageUrl = await SaveImageAsync(imageFile);
                    if (!string.IsNullOrEmpty(imageUrl))
                    {
                        model.ImageUrl = imageUrl;
                    }
                }

                var result = await _productImageService.CreateAsync(model);
                if (result.IsSuccess)
                {
                    TempData["SuccessMessage"] = "Image added successfully.";

                    if (model.VariantId.HasValue)
                    {
                        return RedirectToAction("Index", new { variantId = model.VariantId });
                    }
                    else
                    {
                        return RedirectToAction("Index", new { productId = model.ProductId });
                    }
                }

                TempData["ErrorMessage"] = result.ErrorMessage;
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while creating the image.";
            }
        }

        await PopulateDropdowns();
        return View(model);
    }

    [HttpGet("Edit/{id}")]
    public async Task<IActionResult> Edit(Guid id)
    {
        var result = await _productImageService.GetByIdAsync(id);
        if (!result.IsSuccess)
        {
            TempData["ErrorMessage"] = result.ErrorMessage;
            return RedirectToAction(nameof(Index));
        }

        var updateDto = new ProductImageUpdateDto
        {
            Id = result.Data.Id,
            ImageUrl = result.Data.ImageUrl,
            AltText = result.Data.AltText,
            SortOrder = result.Data.SortOrder,
            IsPrimary = result.Data.IsPrimary,
            VariantId = result.Data.VariantId
        };

        ViewBag.CurrentImageUrl = result.Data.ImageUrl;
        await PopulateDropdowns();
        return View(updateDto);
    }

    [HttpPost("Edit/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, ProductImageUpdateDto model, IFormFile? imageFile)
    {
        if (id != model.Id)
        {
            return BadRequest();
        }

        if (ModelState.IsValid)
        {
            try
            {
                // Handle new file upload
                if (imageFile != null && imageFile.Length > 0)
                {
                    var imageUrl = await SaveImageAsync(imageFile);
                    if (!string.IsNullOrEmpty(imageUrl))
                    {
                        // Delete old image
                        if (!string.IsNullOrEmpty(model.ImageUrl))
                        {
                            await DeleteImageAsync(model.ImageUrl);
                        }

                        model.ImageUrl = imageUrl;
                    }
                }

                var result = await _productImageService.UpdateAsync(model);
                if (result.IsSuccess)
                {
                    TempData["SuccessMessage"] = "Image updated successfully.";

                    if (model.VariantId.HasValue)
                    {
                        return RedirectToAction("Index", new { variantId = model.VariantId });
                    }
                    else
                    {
                        return RedirectToAction("Index", new { productId = result.Data.ProductId });
                    }
                }

                TempData["ErrorMessage"] = result.ErrorMessage;
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while updating the image.";
            }
        }

        await PopulateDropdowns();
        return View(model);
    }

    [HttpPost("Delete/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            var imageResult = await _productImageService.GetByIdAsync(id);
            if (imageResult.IsSuccess)
            {
                // Delete physical file
                await DeleteImageAsync(imageResult.Data.ImageUrl);

                // Delete from database
                var result = await _productImageService.DeleteAsync(id);
                if (result.IsSuccess)
                {
                    TempData["SuccessMessage"] = "Image deleted successfully.";
                }
                else
                {
                    TempData["ErrorMessage"] = result.ErrorMessage;
                }

                // Redirect back to appropriate page
                if (imageResult.Data.VariantId.HasValue)
                {
                    return RedirectToAction("Index", new { variantId = imageResult.Data.VariantId });
                }
                else
                {
                    return RedirectToAction("Index", new { productId = imageResult.Data.ProductId });
                }
            }
            else
            {
                TempData["ErrorMessage"] = "Image not found.";
            }
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = "An error occurred while deleting the image.";
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost("SetPrimary/{id}")]
    public async Task<IActionResult> SetPrimary(Guid id)
    {
        try
        {
            var result = await _productImageService.GetByIdAsync(id);
            if (!result.IsSuccess)
            {
                return Json(new { success = false, message = "Image not found." });
            }

            var updateDto = new ProductImageUpdateDto
            {
                Id = id,
                IsPrimary = true
            };

            var updateResult = await _productImageService.UpdateAsync(updateDto);

            return Json(new
            {
                success = updateResult.IsSuccess,
                message = updateResult.IsSuccess ? "Primary image updated successfully." : updateResult.ErrorMessage
            });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = "An error occurred while updating the primary image." });
        }
    }

    [HttpPost("BulkUpload")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> BulkUpload(Guid productId, Guid? variantId, IFormFileCollection files)
    {
        if (files == null || !files.Any())
        {
            TempData["ErrorMessage"] = "Please select at least one image.";
            return RedirectToAction("Index", new { productId, variantId });
        }

        var successCount = 0;
        var errorCount = 0;

        try
        {
            foreach (var file in files)
            {
                if (file.Length > 0)
                {
                    var imageUrl = await SaveImageAsync(file);
                    if (!string.IsNullOrEmpty(imageUrl))
                    {
                        var createDto = new ProductImageCreateDto
                        {
                            ProductId = productId,
                            VariantId = variantId,
                            ImageUrl = imageUrl,
                            AltText = Path.GetFileNameWithoutExtension(file.FileName),
                            IsPrimary = successCount == 0, // Make first image primary
                            SortOrder = successCount
                        };

                        var result = await _productImageService.CreateAsync(createDto);
                        if (result.IsSuccess)
                        {
                            successCount++;
                        }
                        else
                        {
                            errorCount++;
                            await DeleteImageAsync(imageUrl); // Clean up uploaded file
                        }
                    }
                    else
                    {
                        errorCount++;
                    }
                }
            }

            if (successCount > 0)
            {
                TempData["SuccessMessage"] = $"{successCount} images uploaded successfully.";
            }

            if (errorCount > 0)
            {
                TempData["ErrorMessage"] = $"{errorCount} images failed to upload.";
            }
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = "An error occurred during bulk upload.";
        }

        return RedirectToAction("Index", new { productId, variantId });
    }

    private async Task<string?> SaveImageAsync(IFormFile file)
    {
        try
        {
            if (file == null || file.Length == 0)
                return null;

            // Validate file type
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
            var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (!allowedExtensions.Contains(fileExtension))
            {
                throw new InvalidOperationException("Invalid file type. Only JPG, PNG, and GIF files are allowed.");
            }

            // Validate file size (5MB max)
            if (file.Length > 5 * 1024 * 1024)
            {
                throw new InvalidOperationException("File size cannot exceed 5MB.");
            }

            var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "products");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            var uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return $"/images/products/{uniqueFileName}";
        }
        catch (Exception ex)
        {
            return null;
        }
    }

    private async Task DeleteImageAsync(string imageUrl)
    {
        try
        {
            if (string.IsNullOrEmpty(imageUrl))
                return;

            var filePath = Path.Combine(_webHostEnvironment.WebRootPath,
                imageUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));

            if (File1.Exists(filePath))
            {
                File1.Delete(filePath);
            }
        }
        catch (Exception ex)
        {
            // _logger.LogError(ex, "Error deleting image file: {ImageUrl}", imageUrl);
        }
    }

    private async Task PopulateDropdowns()
    {
        var productsResult = await _productService.GetAllAsync();
        if (productsResult.IsSuccess)
        {
            ViewBag.Products = productsResult.Data?.Select(p => new { p.Id, p.Name }).ToList();
        }
        else
        {
            ViewBag.Products = new List<object>();
        }
    }
}