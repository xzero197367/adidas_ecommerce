using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Adidas.Application.Contracts.ServicesContracts.Tracker;
using Adidas.DTOs.Tracker;
using Adidas.AdminDashboardMVC.ViewModels.Inventory;
using Adidas.Application.Contracts.ServicesContracts.Main;

namespace Adidas.AdminDashboardMVC.Controllers.Inventory
{
    [Authorize(Policy = "EmployeeOrAdmin")]

    public class InventoryController : Controller
    {
        private readonly IInventoryService _inventoryService;
        private readonly ILogger<InventoryController> _logger;
        private readonly IProductService _productService;
        private readonly IProductVariantService _productVariantService;

        public InventoryController(IInventoryService inventoryService, ILogger<InventoryController> logger, IProductService productService, IProductVariantService productVariantService)
        {
            _inventoryService = inventoryService;
            _logger = logger;
            _productService = productService;
            _productVariantService = productVariantService;
        }

        // GET: Inventory
        public async Task<IActionResult> Index()
        {
            try
            {
                var reportResult = await _inventoryService.GenerateInventoryReportAsync();
                var lowStockResult = await _inventoryService.GetLowStockAlertsAsync(10);
                var allProductsResult = await _productService.GetAllAsync();
                var allVariantsResult = await _productVariantService.GetAllAsync();

                // Extract counts safely
                //var totalProducts = (allProductsResult.IsSuccess && allProductsResult.Data != null)
                //    ? allProductsResult.Data.Count()
                //    : 0;

                //var totalVariants = (allVariantsResult.IsSuccess && allVariantsResult.Data != null)
                //    ? allVariantsResult.Data.Count()
                //    : 0;
               
                var viewModel = new InventoryDashboardViewModel();

                if (!reportResult.IsSuccess || reportResult.Data == null)
                {
                    TempData["ErrorMessage"] = reportResult?.ErrorMessage ?? "Unable to load inventory data.";
                    viewModel.Report = new InventoryReportDto
                    {
                        TotalProducts = allProductsResult.Data.Count(),
                        TotalVariants = allVariantsResult.Data.Count(),
                        LowStockVariants = 0,
                        OutOfStockVariants = 0,
                        TotalInventoryValue = 0,
                        ProductStocks = new List<ProductStockDto>()
                    };
                }
                else
                {
                    viewModel.Report = reportResult.Data;

                    // Override totals with the actual dynamic counts
                    viewModel.Report.TotalProducts = allProductsResult.Data.Count();
                    viewModel.Report.TotalVariants = allVariantsResult.Data.Count();

                    if (viewModel.Report.ProductStocks == null)
                    {
                        viewModel.Report.ProductStocks = new List<ProductStockDto>();
                    }
                }

                if (lowStockResult.IsSuccess && lowStockResult.Data != null)
                {
                    viewModel.LowStockAlerts = lowStockResult.Data;
                }
                else
                {
                    viewModel.LowStockAlerts = new List<LowStockAlertDto>();
                }

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading inventory dashboard");
                TempData["ErrorMessage"] = "An error occurred while loading the inventory dashboard.";

                return View(new InventoryDashboardViewModel
                {
                    Report = new InventoryReportDto
                    {
                        TotalProducts = 0,
                        TotalVariants = 0,
                        LowStockVariants = 0,
                        OutOfStockVariants = 0,
                        TotalInventoryValue = 0,
                        ProductStocks = new List<ProductStockDto>()
                    },
                    LowStockAlerts = new List<LowStockAlertDto>()
                });
            }
        }



        // GET: Inventory/LowStock
        public async Task<IActionResult> LowStock(int threshold = 10)
        {
            try
            {
                var result = await _inventoryService.GetLowStockAlertsAsync(threshold);

                var viewModel = new LowStockViewModel
                {
                    Threshold = threshold
                };

                if (!result.IsSuccess || result.Data == null)
                {
                    TempData["ErrorMessage"] = result?.ErrorMessage ?? "Unable to load low stock alerts.";
                    viewModel.Alerts = new List<LowStockAlertDto>();
                }
                else
                {
                    viewModel.Alerts = result.Data;
                }

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading low stock alerts");
                TempData["ErrorMessage"] = "An error occurred while loading low stock alerts.";

                var safeViewModel = new LowStockViewModel
                {
                    Threshold = threshold,
                    Alerts = new List<LowStockAlertDto>()
                };

                return View(safeViewModel);
            }
        }

        // GET: Inventory/Report
        public async Task<IActionResult> Report()
        {
            try
            {
                var result = await _inventoryService.GenerateInventoryReportAsync();

                var viewModel = new InventoryReportViewModel();

                if (!result.IsSuccess || result.Data == null)
                {
                    TempData["ErrorMessage"] = result?.ErrorMessage ?? "Unable to generate inventory report.";
                    // Initialize with empty report
                    viewModel.Report = new InventoryReportDto
                    {
                        TotalProducts = 0,
                        TotalVariants = 0,
                        LowStockVariants = 0,
                        OutOfStockVariants = 0,
                        TotalInventoryValue = 0,
                        ProductStocks = new List<ProductStockDto>()
                    };
                }
                else
                {
                    viewModel.Report = result.Data;
                    // Ensure ProductStocks is never null
                    if (viewModel.Report.ProductStocks == null)
                    {
                        viewModel.Report.ProductStocks = new List<ProductStockDto>();
                    }
                }

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating inventory report");
                TempData["ErrorMessage"] = "An error occurred while generating the report.";

                // Return safe default values
                var safeViewModel = new InventoryReportViewModel
                {
                    Report = new InventoryReportDto
                    {
                        TotalProducts = 0,
                        TotalVariants = 0,
                        LowStockVariants = 0,
                        OutOfStockVariants = 0,
                        TotalInventoryValue = 0,
                        ProductStocks = new List<ProductStockDto>()
                    }
                };

                return View(safeViewModel);
            }
        }

        // GET: Inventory/UpdateStock/{id}
        public IActionResult UpdateStock(Guid id)
        {
            var viewModel = new UpdateStockViewModel
            {
                VariantId = id
            };
            return View(viewModel);
        }

        // POST: Inventory/UpdateStock
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStock(UpdateStockViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var result = await _inventoryService.UpdateStockAsync(model.VariantId, model.NewStock);

                if (result.IsSuccess)
                {
                    TempData["SuccessMessage"] = "Stock updated successfully.";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    ModelState.AddModelError(string.Empty, result.ErrorMessage);
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating stock for variant {VariantId}", model.VariantId);
                ModelState.AddModelError(string.Empty, "An error occurred while updating stock.");
                return View(model);
            }
        }

        // POST: Inventory/ReserveStock
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReserveStock(Guid variantId, int quantity)
        {
            try
            {
                var result = await _inventoryService.ReserveStockAsync(variantId, quantity);

                if (result.IsSuccess)
                {
                    return Json(new { success = true, message = "Stock reserved successfully." });
                }
                else
                {
                    return Json(new { success = false, message = result.ErrorMessage });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reserving stock for variant {VariantId}", variantId);
                return Json(new { success = false, message = "An error occurred while reserving stock." });
            }
        }

        // POST: Inventory/ReleaseStock
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReleaseStock(Guid variantId, int quantity)
        {
            try
            {
                var result = await _inventoryService.ReleaseStockAsync(variantId, quantity);

                if (result.IsSuccess)
                {
                    return Json(new { success = true, message = "Stock released successfully." });
                }
                else
                {
                    return Json(new { success = false, message = result.ErrorMessage });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error releasing stock for variant {VariantId}", variantId);
                return Json(new { success = false, message = "An error occurred while releasing stock." });
            }
        }

        // GET: Inventory/GetLowStockAlerts (AJAX)
        [HttpGet]
        public async Task<IActionResult> GetLowStockAlerts(int threshold = 10)
        {
            try
            {
                var result = await _inventoryService.GetLowStockAlertsAsync(threshold);

                if (result.IsSuccess)
                {
                    return Json(new { success = true, data = result.Data });
                }
                else
                {
                    return Json(new { success = false, message = result.ErrorMessage });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting low stock alerts");
                return Json(new { success = false, message = "An error occurred while getting low stock alerts." });
            }
        }
    }
}