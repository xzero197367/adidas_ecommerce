
using Adidas.DTOs.CommonDTOs;
using Adidas.DTOs.Static;

namespace Adidas.Application.Contracts.ServicesContracts.Static
{

    public interface IAnalyticsService
    {
        Task<OperationResult<DashboardStatsDto>> GetDashboardStatsAsync();
        Task<OperationResult<SalesReportDto>> GenerateSalesReportAsync(DateTime startDate, DateTime endDate);
        Task<OperationResult<IEnumerable<PopularProductDto>>> GetPopularProductsAsync(int count = 10);
        Task<OperationResult<IEnumerable<CategoryPerformanceDto>>> GetCategoryPerformanceAsync();
        Task<OperationResult<CustomerInsightsDto>> GetCustomerInsightsAsync();
        // New methods for dashboard functionality
        Task<OperationResult<IEnumerable<RecentOrderDto>>> GetRecentOrdersAsync(int count = 5);
        Task<OperationResult<IEnumerable<NotificationDto>>> GetDashboardNotificationsAsync();
    }
}
