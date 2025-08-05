
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Adidas.Application.Contracts.RepositoriesContracts.Main;
using Adidas.Application.Contracts.RepositoriesContracts.Operation;
using Adidas.Application.Contracts.RepositoriesContracts.People;
using Adidas.Application.Contracts.RepositoriesContracts.Separator;
using Adidas.Application.Contracts.ServicesContracts.Static;
using Adidas.Context;
using Adidas.DTOs.Static;
using AutoMapper;
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


        public async Task<DashboardStatsDto> GetDashboardStatsAsync()
        {
            try
            {
                var totalProducts = await _productRepository.CountAsync();
                var totalOrders = await _orderRepository.CountAsync();
                var totalCustomers = await _context.Users.CountAsync(u => u.Role == UserRole.Customer);
                var totalRevenue = await _orderRepository.GetTotalSalesAsync();

                var currentMonth = DateTime.UtcNow.Date.AddDays(1 - DateTime.UtcNow.Day);
                var monthlyRevenue = await _orderRepository.GetTotalSalesAsync(currentMonth);

                var lowStockVariants = await _variantRepository.GetLowStockVariantsAsync(10);
                var pendingOrders = await _orderRepository.CountAsync(o => o.OrderStatus == OrderStatus.Pending);

                var allOrders = await _orderRepository.GetAllAsync();
                var averageOrderValue = allOrders.Any() ? allOrders.Average(o => o.TotalAmount) : 0;

                return new DashboardStatsDto
                {
                    TotalProducts = totalProducts,
                    TotalOrders = totalOrders,
                    TotalCustomers = totalCustomers,
                    TotalRevenue = totalRevenue,
                    MonthlyRevenue = monthlyRevenue,
                    LowStockProducts = lowStockVariants.Count(),
                    PendingOrders = pendingOrders,
                    AverageOrderValue = (double)averageOrderValue
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating dashboard stats");
                throw;
            }
        }

        public async Task<SalesReportDto> GenerateSalesReportAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                var orders = await _orderRepository.GetOrdersByDateRangeAsync(startDate, endDate);
                var ordersList = orders.Where(o => o.OrderStatus != OrderStatus.Cancelled).ToList();

                var totalSales = ordersList.Sum(o => o.TotalAmount);
                var totalOrders = ordersList.Count;
                var averageOrderValue = totalOrders > 0 ? totalSales / totalOrders : 0;

                var dailySales = ordersList
                    .GroupBy(o => o.OrderDate.Date)
                    .Select(g => new DailySalesDto
                    {
                        Date = g.Key,
                        Sales = g.Sum(o => o.TotalAmount),
                        Orders = g.Count()
                    })
                    .OrderBy(d => d.Date);

                var categorySales = ordersList
                    .SelectMany(o => o.OrderItems)
                    .GroupBy(oi => oi.Variant.Product.Category.Name)
                    .Select(g => new CategorySalesDto
                    {
                        CategoryName = g.Key,
                        Sales = g.Sum(oi => oi.TotalPrice),
                        ProductsSold = g.Sum(oi => oi.Quantity)
                    });

                return new SalesReportDto
                {
                    StartDate = startDate,
                    EndDate = endDate,
                    TotalSales = totalSales,
                    TotalOrders = totalOrders,
                    AverageOrderValue = averageOrderValue,
                    DailySales = dailySales,
                    CategorySales = categorySales
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating sales report");
                throw;
            }
        }

        public async Task<IEnumerable<PopularProductDto>> GetPopularProductsAsync(int count = 10)
        {
            try
            {
                var allOrders = await _orderRepository.GetAllAsync();
                var popularProducts = allOrders
                    .Where(o => o.OrderStatus != OrderStatus.Cancelled)
                    .SelectMany(o => o.OrderItems)
                    .GroupBy(oi => new { oi.Variant.Product.Id, oi.Variant.Product.Name })
                    .Select(g => new PopularProductDto
                    {
                        ProductId = g.Key.Id,
                        ProductName = g.Key.Name,
                        UnitsSold = g.Sum(oi => oi.Quantity),
                        Revenue = g.Sum(oi => oi.TotalPrice)
                    })
                    .OrderByDescending(p => p.UnitsSold)
                    .Take(count);
                _logger.BeginScope("Getting popular products: {Count}", count);
                return popularProducts;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting popular products");
                throw;
            }
        }

        public async Task<IEnumerable<CategoryPerformanceDto>> GetCategoryPerformanceAsync()
        {
            try
            {
                var categories = await _categoryRepository.GetAllAsync();
                var allOrders = await _orderRepository.GetAllAsync();

                var categoryPerformance = categories.Select(category =>
                {
                    var categoryOrders = allOrders
                        .Where(o => o.OrderStatus != OrderStatus.Cancelled)
                        .SelectMany(o => o.OrderItems)
                        .Where(oi => oi.Variant.Product.CategoryId == category.Id);

                    return new CategoryPerformanceDto
                    {
                        CategoryId = category.Id,
                        CategoryName = category.Name,
                        ProductCount = category.Products.Count,
                        TotalSales = categoryOrders.Sum(oi => oi.TotalPrice),
                        OrderCount = categoryOrders.Select(oi => oi.OrderId).Distinct().Count()
                    };
                })
                .OrderByDescending(cp => cp.TotalSales);

                return categoryPerformance;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting category performance");
                throw;
            }
        }

        public async Task<CustomerInsightsDto> GetCustomerInsightsAsync()
        {
            try
            {
                var allUsers = await _context.Users
                    .Include(u => u.Orders)
                    .Where(u => u.Role == UserRole.Customer)
                    .ToListAsync();
                var customers = allUsers.Where(u => u.Role == UserRole.Customer).ToList();

                var currentMonth = DateTime.UtcNow.Date.AddDays(1 - DateTime.UtcNow.Day);
                var newCustomers = customers.Count(c => c.CreatedAt >= currentMonth);
                var returningCustomers = customers.Count(c => c.Orders.Count > 1);

                var customerOrders = customers.SelectMany(c => c.Orders).Where(o => o.OrderStatus != OrderStatus.Cancelled);
                var averageCustomerValue = customerOrders.Any() ?
                    customerOrders.GroupBy(o => o.UserId).Average(g => g.Sum(o => o.TotalAmount)) : 0;

                var customerSegments = new List<CustomerSegmentDto>
                {
                    new CustomerSegmentDto
                    {
                        SegmentName = "New Customers",
                        CustomerCount = newCustomers,
                        AverageOrderValue = customerOrders
                            .Where(o => customers.First(c => c.Id == o.UserId).CreatedAt >= currentMonth)
                            .DefaultIfEmpty()
                            .Average(o => o?.TotalAmount ?? 0)
                    },
                    new CustomerSegmentDto
                    {
                        SegmentName = "Returning Customers",
                        CustomerCount = returningCustomers,
                        AverageOrderValue = customerOrders
                            .Where(o => customers.First(c => c.Id == o.UserId).Orders.Count > 1)
                            .DefaultIfEmpty()
                            .Average(o => o?.TotalAmount ?? 0)
                    }
                };

                return new CustomerInsightsDto
                {
                    NewCustomers = newCustomers,
                    ReturningCustomers = returningCustomers,
                    AverageCustomerValue = averageCustomerValue,
                    CustomerSegments = customerSegments
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting customer insights");
                throw;
            }
        }

        // NEW METHOD: Get Recent Orders
        public async Task<IEnumerable<RecentOrderDto>> GetRecentOrdersAsync(int count = 5)
        {
            try
            {
                var recentOrders = await _orderRepository.GetAllAsync();
                var orderDtos = recentOrders.Select(o => new RecentOrderDto
                {
                    OrderId = o.Id,
                    CustomerName = o.User?.UserName ,
                    TotalAmount = o.TotalAmount,
                    OrderStatus = o.OrderStatus.ToString(),
                    OrderDate = o.OrderDate
                }).ToList();

                return orderDtos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting recent orders");
                throw;
            }
        }

        // NEW METHOD: Get Dashboard Notifications
        public async Task<IEnumerable<NotificationDto>> GetDashboardNotificationsAsync()
        {
            try
            {
                var notifications = new List<NotificationDto>();
                var stats = await GetDashboardStatsAsync();

                // Low stock notification
                if (stats.LowStockProducts > 0)
                {
                    notifications.Add(new NotificationDto
                    {
                        Type = "warning",
                        Title = "Low Stock Alert",
                        Message = $"{stats.LowStockProducts} products are running low on stock and need restocking.",
                        ActionText = "View Low Stock Items",
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
                        Message = $"{stats.PendingOrders} orders are waiting for processing.",
                        ActionText = "Process Orders",
                        CreatedAt = DateTime.UtcNow
                    });
                }

                // New customers notification
                var customerInsights = await GetCustomerInsightsAsync();
                if (customerInsights.NewCustomers > 0)
                {
                    notifications.Add(new NotificationDto
                    {
                        Type = "success",
                        Title = "New Customers",
                        Message = $"{customerInsights.NewCustomers} new customers joined this month!",
                        ActionText = "View Customers",
                        CreatedAt = DateTime.UtcNow
                    });
                }

                return notifications.OrderByDescending(n => n.CreatedAt);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting dashboard notifications");
                throw;
            }
        }
    }
}