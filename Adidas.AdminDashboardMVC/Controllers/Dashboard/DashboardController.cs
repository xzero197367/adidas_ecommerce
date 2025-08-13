using Adidas.AdminDashboardMVC.ViewModels.Dashboard;
using Adidas.Application.Contracts.ServicesContracts.Operation;
using Adidas.Application.Contracts.ServicesContracts.Static;
using Adidas.DTOs.Static;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Adidas.AdminDashboardMVC.Controllers.Dashboard
{
    [Authorize(Policy = "EmployeeOrAdmin")]
    public class DashboardController : Controller
    {
        private readonly IAnalyticsService _analyticsService;
        private readonly ILogger<DashboardController> _logger;

        public DashboardController(
            IAnalyticsService analyticsService,
            IOrderService orderService,
            ILogger<DashboardController> logger)
        {
            _analyticsService = analyticsService;
            
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                _logger.LogInformation("Loading dashboard data...");

                // Get dashboard statistics
                var dashboardStats = await _analyticsService.GetDashboardStatsAsync();

                // Get sales report for the last 30 days
                var endDate = DateTime.UtcNow.Date;
                var startDate = endDate.AddDays(-30);
                var salesReport = await _analyticsService.GenerateSalesReportAsync(startDate, endDate);

                // Get popular products (top 5)
                var popularProducts = await _analyticsService.GetPopularProductsAsync(5);

                // Get category performance
                var categoryPerformance = await _analyticsService.GetCategoryPerformanceAsync();

                // Get customer insights
                var customerInsights = await _analyticsService.GetCustomerInsightsAsync();

                // Create view model
                var viewModel = new DashboardViewModel
                {
                    Stats = dashboardStats.IsSuccess && dashboardStats.Data != null
                        ? dashboardStats.Data
                        : new DashboardStatsDto(),
                    SalesReport = salesReport.IsSuccess && salesReport.Data != null
                        ? salesReport.Data
                        : new SalesReportDto { DailySales = new List<DailySalesDto>() },
                    PopularProducts = popularProducts.IsSuccess && popularProducts.Data != null
                        ? popularProducts.Data.ToList()
                        : new List<PopularProductDto>(),
                    CategoryPerformance = categoryPerformance.IsSuccess && categoryPerformance.Data != null
                        ? categoryPerformance.Data.Take(6).ToList()
                        : new List<CategoryPerformanceDto>(),
                    CustomerInsights = customerInsights.IsSuccess && customerInsights.Data != null
                        ? customerInsights.Data
                        : new CustomerInsightsDto(),
                    CurrentTimeRange = "Last 30 days",
                    LastUpdated = DateTime.UtcNow
                };

                _logger.LogInformation("Dashboard data loaded successfully");
                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading dashboard data");
                TempData["Error"] = "Unable to load dashboard data. Please try again.";

                // Return empty view model to prevent null reference exceptions
                return View(new DashboardViewModel
                {
                    Stats = new DashboardStatsDto(),
                    SalesReport = new SalesReportDto { DailySales = new List<DailySalesDto>() },
                    PopularProducts = new List<PopularProductDto>(),
                    CategoryPerformance = new List<CategoryPerformanceDto>(),
                    CustomerInsights = new CustomerInsightsDto(),
                    CurrentTimeRange = "Last 30 days",
                    LastUpdated = DateTime.UtcNow
                });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetSalesData(int days = 30)
        {
            try
            {
                var endDate = DateTime.UtcNow.Date;
                var startDate = endDate.AddDays(-days);
                var salesReport = await _analyticsService.GenerateSalesReportAsync(startDate, endDate);

                if (salesReport.IsSuccess && salesReport.Data?.DailySales != null)
                {
                    var chartData = salesReport.Data.DailySales.Select(d => new
                    {
                        date = d.Date.ToString("yyyy-MM-dd"),
                        sales = d.Sales,
                        orders = d.Orders
                    });

                    return Json(new { success = true, data = chartData });
                }

                return Json(new { success = false, error = "Unable to load sales data" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading sales data");
                return Json(new { success = false, error = "Unable to load sales data" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetRecentOrders(int count = 5)
        {
            try
            {
                var recentOrdersResult = await _analyticsService.GetRecentOrdersAsync(count);

                if (recentOrdersResult.IsSuccess && recentOrdersResult.Data != null)
                {
                    return Json(new { success = true, data = recentOrdersResult.Data });
                }

                return Json(new { success = false, error = "Unable to load recent orders" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading recent orders");
                return Json(new { success = false, error = "Unable to load recent orders" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetDashboardNotifications()
        {
            try
            {
                var notificationsResult = await _analyticsService.GetDashboardNotificationsAsync();

                if (notificationsResult.IsSuccess && notificationsResult.Data != null)
                {
                    return Json(new { success = true, data = notificationsResult.Data });
                }

                return Json(new { success = false, error = "Unable to load notifications" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading notifications");
                return Json(new { success = false, error = "Unable to load notifications" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetCategoryData()
        {
            try
            {
                var categoryPerformance = await _analyticsService.GetCategoryPerformanceAsync();

                if (categoryPerformance.IsSuccess && categoryPerformance.Data != null)
                {
                    var chartData = categoryPerformance.Data.Select(c => new
                    {
                        categoryName = c.CategoryName,
                        totalSales = c.TotalSales,
                        orderCount = c.OrderCount
                    });

                    return Json(new { success = true, data = chartData });
                }

                return Json(new { success = false, error = "Unable to load category data" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading category data");
                return Json(new { success = false, error = "Unable to load category data" });
            }
        }
    }
}