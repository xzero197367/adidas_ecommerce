using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Adidas.DTOs.Static;

namespace Adidas.Application.Contracts.ServicesContracts.Static
{

    public interface IAnalyticsService
    {
        Task<DashboardStatsDto> GetDashboardStatsAsync();
        Task<SalesReportDto> GenerateSalesReportAsync(DateTime startDate, DateTime endDate);
        Task<IEnumerable<PopularProductDto>> GetPopularProductsAsync(int count = 10);
        Task<IEnumerable<CategoryPerformanceDto>> GetCategoryPerformanceAsync();
        Task<CustomerInsightsDto> GetCustomerInsightsAsync();
        // New methods for dashboard functionality
        Task<IEnumerable<RecentOrderDto>> GetRecentOrdersAsync(int count = 5);
        Task<IEnumerable<NotificationDto>> GetDashboardNotificationsAsync();
    }
}
