using Adidas.AdminDashboardMVC.ViewModels.Dashboard;
using Adidas.Application.Contracts.RepositoriesContracts.Operation;
using Adidas.Application.Contracts.ServicesContracts.Static;
using Adidas.DTOs.Static;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Adidas.AdminDashboardMVC.Controllers.Dashboard
{
    [Authorize(Policy = "EmployeeOrAdmin")]
    public class DashboardController : Controller
    {
        private readonly IAnalyticsService _analyticsService;
        private readonly IOrderRepository _orderRepository;
        private readonly ILogger<DashboardController> _logger;

        public DashboardController(
            IAnalyticsService analyticsService,
            IOrderRepository orderRepository,
            ILogger<DashboardController> logger)
        {
            _analyticsService = analyticsService;
            _orderRepository = orderRepository;
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
                    Stats = dashboardStats,
                    SalesReport = salesReport,
                    PopularProducts = popularProducts.ToList(),
                    CategoryPerformance = categoryPerformance.Take(6).ToList(),
                    CustomerInsights = customerInsights,
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
                return View(new DashboardViewModel());
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

                var chartData = salesReport.DailySales.Select(d => new
                {
                    date = d.Date.ToString("yyyy-MM-dd"),
                    sales = d.Sales,
                    orders = d.Orders
                });

                return Json(chartData);
            }
            catch (Exception)
            {
                return Json(new { error = "Unable to load sales data" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetRecentOrders(int count = 5)
        {
            try
            {
                var recentOrders = await _orderRepository.GetAll().ToListAsync();
                var orderDtos = recentOrders.Select(o => new RecentOrderDto
                {
                    OrderId = o.Id,
                    CustomerName = o.User?.UserName,
                    TotalAmount = o.TotalAmount,
                    OrderStatus = o.OrderStatus.ToString(),
                    OrderDate = o.OrderDate
                }).ToList();

                return Json(orderDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading recent orders");
                return Json(new { error = "Unable to load recent orders" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetDashboardNotifications()
        {
            try
            {
                var notifications = new List<NotificationDto>();
                var stats = await _analyticsService.GetDashboardStatsAsync();

                // Low stock notification
                if (stats.LowStockProducts > 0)
                {
                    notifications.Add(new NotificationDto
                    {
                        Type = "warning",
                        Title = "Low Stock Alert",
                        Message = $"{stats.LowStockProducts} products are running low on stock",
                        //ActionUrl = Url.Action("LowStock", "Product"),
                        ActionText = "View Products",
                        CreatedAt = DateTime.UtcNow
                    });
                }

                // Pending orders notification
                if (stats.PendingOrders > 0)
                {
                    notifications.Add(new NotificationDto
                    {
                        Type = "info",
                        Title = "Pending Orders",
                        Message = $"{stats.PendingOrders} orders are waiting for processing",
                        //ActionUrl = Url.Action("Pending", "Order"),
                        ActionText = "Process Orders",
                        CreatedAt = DateTime.UtcNow
                    });
                }

                return Json(notifications);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading notifications");
                return Json(new { error = "Unable to load notifications" });
            }
        }
    }
}