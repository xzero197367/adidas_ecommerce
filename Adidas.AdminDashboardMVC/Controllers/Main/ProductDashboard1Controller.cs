using Microsoft.AspNetCore.Mvc;
using Adidas.Application.Contracts.ServicesContracts.Main;
using Adidas.Application.Contracts.ServicesContracts.Separator;
using Adidas.Application.Contracts.ServicesContracts.Operation;

namespace Adidas.AdminDashboardMVC.Controllers.Main;

[Route("Admin")]
public class ProductDashboard1Controller : Controller
{
    private readonly IProductService _productService;
    private readonly IProductVariantService _productVariantService;
    private readonly ICategoryService _categoryService;
    private readonly IBrandService _brandService;
    private readonly IOrderService _orderService;

    public ProductDashboard1Controller(
        IProductService productService,
        IProductVariantService productVariantService,
        ICategoryService categoryService,
        IBrandService brandService,
        IOrderService orderService)
    {
        _productService = productService;
        _productVariantService = productVariantService;
        _categoryService = categoryService;
        _brandService = brandService;
        _orderService = orderService;
    }

    [HttpGet]
    [HttpGet("Dashboard")]
    public async Task<IActionResult> Index()
    {
        try
        {
            var dashboardData = new ProductDashboardViewModel();

            // Get basic counts
            await PopulateBasicStats(dashboardData);

            // Get recent products
            await PopulateRecentProducts(dashboardData);

            // Get low stock alerts
            await PopulateLowStockAlerts(dashboardData);

            // Get sales overview (if order service is available)
            await PopulateSalesOverview(dashboardData);

            return View(dashboardData);
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = "An error occurred while loading the dashboard.";
            return View(new ProductDashboardViewModel());
        }
    }

    [HttpGet("QuickStats")]
    public async Task<IActionResult> GetQuickStats()
    {
        try
        {
            var stats = new
            {
                totalProducts = await GetTotalProducts(),
                totalVariants = await GetTotalVariants(),
                lowStockItems = await GetLowStockCount(),
                outOfStockItems = await GetOutOfStockCount(),
                totalCategories = await GetTotalCategories(),
                totalBrands = await GetTotalBrands()
            };

            return Json(stats);
        }
        catch (Exception ex)
        {
            return Json(new { error = "Failed to load statistics" });
        }
    }

    [HttpGet("RecentActivity")]
    public async Task<IActionResult> GetRecentActivity()
    {
        try
        {
            var activities = new List<object>();

            // Recent products
            var productsResult = await _productService.GetAllAsync();
            if (productsResult.IsSuccess)
            {
                var recentProducts = productsResult.Data
                    .OrderByDescending(p => p.CreatedAt)
                    .Take(5)
                    .Select(p => new
                    {
                        type = "product_created",
                        title = "New Product Created",
                        description = $"{p.Name} was added to catalog",
                        timestamp = p.CreatedAt,
                        link = $"/Admin/Product/Details/{p.Id}"
                    });
                activities.AddRange(recentProducts);
            }

            // Recent variants
            var variantsResult = await _productVariantService.GetAllAsync();
            if (variantsResult.IsSuccess)
            {
                var recentVariants = variantsResult.Data
                    .OrderByDescending(v => v.CreatedAt)
                    .Take(3)
                    .Select(v => new
                    {
                        type = "variant_created",
                        title = "New Variant Created",
                        description = $"{v.Product?.Name} - {v.Color} ({v.Size}) was added",
                        timestamp = v.CreatedAt,
                        link = $"/Admin/ProductVariant/Details/{v.Id}"
                    });
                activities.AddRange(recentVariants);
            }

            // Sort by timestamp and take top 10
            var sortedActivities = activities
                .OrderByDescending(a =>
                    ((DateTime?)a.GetType().GetProperty("timestamp")?.GetValue(a)) ?? DateTime.MinValue)
                .Take(10);

            return Json(sortedActivities);
        }
        catch (Exception ex)
        {
            return Json(new List<object>());
        }
    }

    [HttpGet("StockAlerts")]
    public async Task<IActionResult> GetStockAlerts()
    {
        try
        {
            var alerts = new List<object>();

            var variantsResult = await _productVariantService.GetAllAsync();
            if (variantsResult.IsSuccess)
            {
                var lowStockVariants = variantsResult.Data
                    .Where(v => v.StockQuantity <= 10)
                    .OrderBy(v => v.StockQuantity)
                    .Select(v => new
                    {
                        id = v.Id,
                        productName = v.Product?.Name,
                        variant = $"{v.Color} - {v.Size}",
                        sku = v.Sku,
                        stock = v.StockQuantity,
                        status = v.StockQuantity == 0 ? "out_of_stock" : "low_stock",
                        link = $"/Admin/ProductVariant/Details/{v.Id}"
                    });

                alerts.AddRange(lowStockVariants);
            }

            return Json(alerts);
        }
        catch (Exception ex)
        {
            return Json(new List<object>());
        }
    }

    private async Task PopulateBasicStats(ProductDashboardViewModel model)
    {
        model.TotalProducts = await GetTotalProducts();
        model.TotalVariants = await GetTotalVariants();
        model.TotalCategories = await GetTotalCategories();
        model.TotalBrands = await GetTotalBrands();
        model.LowStockItems = await GetLowStockCount();
        model.OutOfStockItems = await GetOutOfStockCount();
    }

    private async Task PopulateRecentProducts(ProductDashboardViewModel model)
    {
        var result = await _productService.GetAllAsync();
        if (result.IsSuccess)
        {
            model.RecentProducts = result.Data
                .OrderByDescending(p => p.CreatedAt)
                .Take(5)
                .ToList();
        }
    }

    private async Task PopulateLowStockAlerts(ProductDashboardViewModel model)
    {
        var result = await _productVariantService.GetAllAsync();
        if (result.IsSuccess)
        {
            model.LowStockAlerts = result.Data
                .Where(v => v.StockQuantity <= 10)
                .OrderBy(v => v.StockQuantity)
                .Take(10)
                .ToList();
        }
    }

    private async Task PopulateSalesOverview(ProductDashboardViewModel model)
    {
        // This would be implemented if you have order/sales data
        // For now, we'll set some placeholder data
        model.TodaySales = 0;
        model.WeeklySales = 0;
        model.MonthlySales = 0;
        model.TotalOrders = 0;
        model.PendingOrders = 0;
    }

    private async Task<int> GetTotalProducts()
    {
        var result = await _productService.GetAllAsync();
        return result.IsSuccess ? result.Data.Count() : 0;
    }

    private async Task<int> GetTotalVariants()
    {
        var result = await _productVariantService.GetAllAsync();
        return result.IsSuccess ? result.Data.Count() : 0;
    }

    private async Task<int> GetTotalCategories()
    {
        var result = await _categoryService.GetAllAsync();
        return result.IsSuccess ? result.Data.Count() : 0;
    }

    private async Task<int> GetTotalBrands()
    {
        var result = await _brandService.GetAllAsync();
        return result.IsSuccess ? result.Data.Count() : 0;
    }

    private async Task<int> GetLowStockCount()
    {
        var result = await _productVariantService.GetAllAsync();
        if (!result.IsSuccess) return 0;

        return result.Data.Count(v => v.StockQuantity > 0 && v.StockQuantity <= 10);
    }

    private async Task<int> GetOutOfStockCount()
    {
        var result = await _productVariantService.GetAllAsync();
        if (!result.IsSuccess) return 0;

        return result.Data.Count(v => v.StockQuantity == 0);
    }
}

// Dashboard ViewModel
public class ProductDashboardViewModel
{
    // Basic Stats
    public int TotalProducts { get; set; }
    public int TotalVariants { get; set; }
    public int TotalCategories { get; set; }
    public int TotalBrands { get; set; }
    public int LowStockItems { get; set; }
    public int OutOfStockItems { get; set; }

    // Sales Data
    public decimal TodaySales { get; set; }
    public decimal WeeklySales { get; set; }
    public decimal MonthlySales { get; set; }
    public int TotalOrders { get; set; }
    public int PendingOrders { get; set; }

    // Recent Data
    public IEnumerable<Adidas.DTOs.Main.Product_DTOs.ProductDto> RecentProducts { get; set; } =
        new List<Adidas.DTOs.Main.Product_DTOs.ProductDto>();

    public IEnumerable<Adidas.DTOs.Main.Product_Variant_DTOs.ProductVariantDto> LowStockAlerts { get; set; } =
        new List<Adidas.DTOs.Main.Product_Variant_DTOs.ProductVariantDto>();

    // Chart Data
    public List<ChartDataPoint> SalesChartData { get; set; } = new List<ChartDataPoint>();
    public List<ChartDataPoint> CategoryDistribution { get; set; } = new List<ChartDataPoint>();
    public List<ChartDataPoint> StockStatusData { get; set; } = new List<ChartDataPoint>();
}

public class ChartDataPoint
{
    public string Label { get; set; } = string.Empty;
    public decimal Value { get; set; }
    public string Color { get; set; } = string.Empty;
}