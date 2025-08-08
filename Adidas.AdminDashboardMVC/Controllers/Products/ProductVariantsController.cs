using Adidas.Application.Contracts.ServicesContracts.Main;
using Adidas.Application.Services.Main;
using Adidas.DTOs.Main.Product_Variant_DTOs;
using Adidas.Models.Main;
using Microsoft.AspNetCore.Mvc;

namespace Adidas.AdminDashboardMVC.Controllers.Products
{
    public class ProductVariantsController : Controller
    {
        private readonly IProductVariantService _productVariantService;

        public ProductVariantsController(IProductVariantService productVariantService)
        {
            _productVariantService = productVariantService;
        }
        public async Task<IActionResult> Index(Guid variantId)
        {
            var variant = await _productVariantService.GetByIdAsync(variantId, v => v.Images, v => v.Product);
            if (variant == null)
                return NotFound();

            ViewBag.VariantId = variantId;
            return View(variant);
        }


        // POST: /ProductVariant/AddImage
        [HttpPost]
        public async Task<IActionResult> AddImage(Guid variantId, IFormFile imageFile)
        {
            if (imageFile == null || imageFile.Length == 0)
            {
                TempData["Error"] = "Please select an image file.";
                return RedirectToAction(nameof(Index), new { variantId });
            }

            var result = await _productVariantService.AddImageAsync(variantId, imageFile);
            if (!result)
            {
                TempData["Error"] = "Failed to upload the image.";
            }
            else
            {
                TempData["Success"] = "Image uploaded successfully.";
            }

            return RedirectToAction(nameof(Index), new { variantId });
        }


        public async Task<IActionResult> SearchBySku(string sku)
        {
            if (string.IsNullOrWhiteSpace(sku))
            {
                TempData["Error"] = "Please enter a SKU to search.";
                return RedirectToAction("Index", "Products");
            }

            var variant = await _productVariantService.GetBySkuAsync(sku);
            if (variant == null)
            {
                TempData["Error"] = $"No variant found with SKU: {sku}";
                return RedirectToAction("Index", "Products");
            }
            return RedirectToAction(nameof(Index), new { variantId = variant.Id });
        }

    }
}