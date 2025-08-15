using Adidas.Application.Contracts.RepositoriesContracts.Main;
using Adidas.Application.Contracts.RepositoriesContracts.Operation;
using Adidas.Application.Contracts.RepositoriesContracts.Separator;
using Adidas.Application.Contracts.ServicesContracts.Static;
using Adidas.Context;
using Adidas.DTOs.CommonDTOs;
using Adidas.DTOs.Static;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Models.People;

namespace Adidas.Application.Services.Static
{
    public class AnalyticsService : IAnalyticsService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IProductRepository _productRepository;
        private readonly UserManager<User> _userManager;
        private readonly AdidasDbContext _context;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IProductVariantRepository _variantRepository;
        private readonly ILogger<AnalyticsService> _logger;

        public AnalyticsService(
            IOrderRepository orderRepository,
            IProductRepository productRepository,
            ICategoryRepository categoryRepository,
            IProductVariantRepository variantRepository,
            UserManager<User> userManager,
            AdidasDbContext context,
            ILogger<AnalyticsService> logger)
        {
            _orderRepository = orderRepository;
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
            _variantRepository = variantRepository;
            _userManager = userManager;
            _context = context;
            _logger = logger;
        }

        public async Task<OperationResult<DashboardStatsDto>> GetDashboardStatsAsync()
        {
            try
            {
                // Use direct DbContext queries to avoid IQueryable async issues
                var totalProducts = await _context.Products.CountAsync();
                var totalOrders = await _context.Orders.CountAsync();
                var totalCustomers = await _context.Users.CountAsync(u => u.Role == UserRole.Customer);

                // Calculate total revenue using DbContext directly
                var totalRevenue = await _context.Orders
                    .Where(o => o.OrderStatus != OrderStatus.Cancelled)
                    .SumAsync(o => o.TotalAmount);

                // Calculate monthly revenue
                var currentMonth = DateTime.UtcNow.Date.AddDays(1 - DateTime.UtcNow.Day);
                var monthlyRevenue = await _context.Orders
                    .Where(o => o.OrderDate >= currentMonth && o.OrderStatus != OrderStatus.Cancelled)
                    .SumAsync(o => o.TotalAmount);

                // Get low stock variants count
                var lowStockCount = await _context.ProductVariants
                    .CountAsync(v => v.StockQuantity <= 10);

                // Get pending orders count
                var pendingOrders = await _context.Orders
                    .CountAsync(o => o.OrderStatus == OrderStatus.Pending);

                // Calculate average order value
                var completedOrders = await _context.Orders
                    .Where(o => o.OrderStatus != OrderStatus.Cancelled)
                    .ToListAsync();

                var averageOrderValue = completedOrders.Any() ? completedOrders.Average(o => o.TotalAmount) : 0;

                var result = new DashboardStatsDto
                {
                    TotalProducts = totalProducts,
                    TotalOrders = totalOrders,
                    TotalCustomers = totalCustomers,
                    TotalRevenue = totalRevenue,
                    MonthlyRevenue = monthlyRevenue,
                    LowStockProducts = lowStockCount,
                    PendingOrders = pendingOrders,
                    AverageOrderValue = (double)averageOrderValue
                };

                return OperationResult<DashboardStatsDto>.Success(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating dashboard stats");
                return OperationResult<DashboardStatsDto>.Fail(ex.Message);
            }
        }

        public async Task<OperationResult<SalesReportDto>> GenerateSalesReportAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                // Use DbContext directly to get orders by date range
                var orders = await _context.Orders
                    .Include(o => o.OrderItems)
                        .ThenInclude(oi => oi.Variant)
                            .ThenInclude(v => v.Product)
                                .ThenInclude(p => p.Category)
                    .Where(o => o.OrderDate >= startDate && o.OrderDate <= endDate)
                    .ToListAsync();

                var validOrders = orders.Where(o => o.OrderStatus != OrderStatus.Cancelled).ToList();

                var totalSales = validOrders.Sum(o => o.TotalAmount);
                var totalOrders = validOrders.Count;
                var averageOrderValue = totalOrders > 0 ? totalSales / totalOrders : 0;

                var dailySales = validOrders
                    .GroupBy(o => o.OrderDate.Date)
                    .Select(g => new DailySalesDto
                    {
                        Date = g.Key,
                        Sales = g.Sum(o => o.TotalAmount),
                        Orders = g.Count()
                    })
                    .OrderBy(d => d.Date)
                    .ToList();

                var categorySales = validOrders
                    .SelectMany(o => o.OrderItems)
                    .Where(oi => oi.Variant?.Product?.Category != null)
                    .GroupBy(oi => oi.Variant.Product.Category.Name)
                    .Select(g => new CategorySalesDto
                    {
                        CategoryName = g.Key,
                        Sales = g.Sum(oi => oi.TotalPrice),
                        ProductsSold = g.Sum(oi => oi.Quantity)
                    })
                    .ToList();

                var result = new SalesReportDto
                {
                    StartDate = startDate,
                    EndDate = endDate,
                    TotalSales = totalSales,
                    TotalOrders = totalOrders,
                    AverageOrderValue = averageOrderValue,
                    DailySales = dailySales,
                    CategorySales = categorySales
                };

                return OperationResult<SalesReportDto>.Success(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating sales report");
                return OperationResult<SalesReportDto>.Fail(ex.Message);
            }
        }

        public async Task<OperationResult<IEnumerable<PopularProductDto>>> GetPopularProductsAsync(int count = 10)
        {
            try
            {
                var popularProducts = await _context.Orders
                    .Where(o => o.OrderStatus != OrderStatus.Cancelled)
                    .SelectMany(o => o.OrderItems)
                    .Include(oi => oi.Variant)
                        .ThenInclude(v => v.Product)
                    .GroupBy(oi => new { oi.Variant.Product.Id, oi.Variant.Product.Name })
                    .Select(g => new PopularProductDto
                    {
                        ProductId = g.Key.Id,
                        ProductName = g.Key.Name,
                        UnitsSold = g.Sum(oi => oi.Quantity),
                        Revenue = g.Sum(oi => oi.TotalPrice)
                    })
                    .OrderByDescending(p => p.UnitsSold)
                    .Take(count)
                    .ToListAsync();

                return OperationResult<IEnumerable<PopularProductDto>>.Success(popularProducts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting popular products");
                return OperationResult<IEnumerable<PopularProductDto>>.Fail(ex.Message);
            }
        }

        public async Task<OperationResult<IEnumerable<CategoryPerformanceDto>>> GetCategoryPerformanceAsync()
        {
            try
            {
                var categoryPerformance = await _context.Categories
                    .Include(c => c.Products)
                    .Select(category => new CategoryPerformanceDto
                    {
                        CategoryId = category.Id,
                        CategoryName = category.Name,
                        ProductCount = category.Products.Count,
                        TotalSales = _context.Orders
                            .Where(o => o.OrderStatus != OrderStatus.Cancelled)
                            .SelectMany(o => o.OrderItems)
                            .Where(oi => oi.Variant.Product.CategoryId == category.Id)
                            .Sum(oi => oi.TotalPrice),
                        OrderCount = _context.Orders
                            .Where(o => o.OrderStatus != OrderStatus.Cancelled)
                            .SelectMany(o => o.OrderItems)
                            .Where(oi => oi.Variant.Product.CategoryId == category.Id)
                            .Select(oi => oi.OrderId)
                            .Distinct()
                            .Count()
                    })
                    .OrderByDescending(cp => cp.TotalSales)
                    .ToListAsync();

                return OperationResult<IEnumerable<CategoryPerformanceDto>>.Success(categoryPerformance);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting category performance");
                return OperationResult<IEnumerable<CategoryPerformanceDto>>.Fail(ex.Message);
            }
        }

        public async Task<OperationResult<CustomerInsightsDto>> GetCustomerInsightsAsync()
        {
            try
            {
                var customers = await _context.Users
                    .Include(u => u.Orders)
                    .Where(u => u.Role == UserRole.Customer)
                    .ToListAsync();

                var currentMonth = DateTime.UtcNow.Date.AddDays(1 - DateTime.UtcNow.Day);
                var newCustomers = customers.Count(c => c.CreatedAt >= currentMonth);
                var returningCustomers = customers.Count(c => c.Orders.Count > 1);

                var customerOrders = customers
                    .SelectMany(c => c.Orders)
                    .Where(o => o.OrderStatus != OrderStatus.Cancelled)
                    .ToList();

                var averageCustomerValue = customerOrders.Any() ?
                    customerOrders.GroupBy(o => o.UserId).Average(g => g.Sum(o => o.TotalAmount)) : 0;

                var customerSegments = new List<CustomerSegmentDto>
                {
                    new CustomerSegmentDto
                    {
                        SegmentName = "New Customers",
                        CustomerCount = newCustomers,
                        AverageOrderValue = customerOrders
                            .Where(o => customers.Any(c => c.Id == o.UserId && c.CreatedAt >= currentMonth))
                            .DefaultIfEmpty()
                            .Average(o => o?.TotalAmount ?? 0)
                    },
                    new CustomerSegmentDto
                    {
                        SegmentName = "Returning Customers",
                        CustomerCount = returningCustomers,
                        AverageOrderValue = customerOrders
                            .Where(o => customers.Any(c => c.Id == o.UserId && c.Orders.Count > 1))
                            .DefaultIfEmpty()
                            .Average(o => o?.TotalAmount ?? 0)
                    }
                };

                var result = new CustomerInsightsDto
                {
                    NewCustomers = newCustomers,
                    ReturningCustomers = returningCustomers,
                    AverageCustomerValue = averageCustomerValue,
                    CustomerSegments = customerSegments
                };

                return OperationResult<CustomerInsightsDto>.Success(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting customer insights");
                return OperationResult<CustomerInsightsDto>.Fail(ex.Message);
            }
        }

        public async Task<OperationResult<IEnumerable<RecentOrderDto>>> GetRecentOrdersAsync(int count = 5)
        {
            try
            {
                var recentOrders = await _context.Orders
                    .Include(o => o.User)
                    .OrderByDescending(o => o.OrderDate)
                    .Take(count)
                    .ToListAsync();

                var orderDtos = recentOrders.Select(o => new RecentOrderDto
                {
                    OrderId = o.Id,
                    CustomerName = o.User?.UserName ?? "Unknown Customer",
                    TotalAmount = o.TotalAmount,
                    OrderStatus = o.OrderStatus.ToString(),
                    OrderDate = o.OrderDate
                }).ToList();

                return OperationResult<IEnumerable<RecentOrderDto>>.Success(orderDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting recent orders");
                return OperationResult<IEnumerable<RecentOrderDto>>.Fail(ex.Message);
            }
        }

        public async Task<OperationResult<IEnumerable<NotificationDto>>> GetDashboardNotificationsAsync()
        {
            try
            {
                var notifications = new List<NotificationDto>();
                var stats = await GetDashboardStatsAsync();

                if (!stats.IsSuccess || stats.Data == null)
                {
                    return OperationResult<IEnumerable<NotificationDto>>.Success(notifications);
                }

                // Low stock notification
                if (stats.Data.LowStockProducts > 0)
                {
                    notifications.Add(new NotificationDto
                    {
                        Type = "warning",
                        Title = "Low Stock Alert",
                        Message = $"{stats.Data.LowStockProducts} products are running low on stock and need restocking.",
                        ActionText = "View Low Stock Items",
                        CreatedAt = DateTime.UtcNow
                    });
                }

                // Pending orders notification
                if (stats.Data.PendingOrders > 0)
                {
                    notifications.Add(new NotificationDto
                    {
                        Type = "info",
                        Title = "Pending Orders",
                        Message = $"{stats.Data.PendingOrders} orders are waiting for processing.",
                        ActionText = "Process Orders",
                        CreatedAt = DateTime.UtcNow
                    });
                }

                // New customers notification
                var customerInsights = await GetCustomerInsightsAsync();
                if (customerInsights.IsSuccess && customerInsights.Data != null && customerInsights.Data.NewCustomers > 0)
                {
                    notifications.Add(new NotificationDto
                    {
                        Type = "success",
                        Title = "New Customers",
                        Message = $"{customerInsights.Data.NewCustomers} new customers joined this month!",
                        ActionText = "View Customers",
                        CreatedAt = DateTime.UtcNow
                    });
                }

                var result = notifications.OrderByDescending(n => n.CreatedAt);
                return OperationResult<IEnumerable<NotificationDto>>.Success(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting dashboard notifications");
                return OperationResult<IEnumerable<NotificationDto>>.Fail(ex.Message);
            }
        }
    }
}