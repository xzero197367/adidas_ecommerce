using Adidas.DTOs.Static;

namespace Adidas.AdminDashboardMVC.ViewModels.Dashboard
{
    public class DashboardViewModel
    {
        public DashboardStatsDto Stats { get; set; } = new DashboardStatsDto();
        public SalesReportDto SalesReport { get; set; } = new SalesReportDto();
        public List<PopularProductDto> PopularProducts { get; set; } = new List<PopularProductDto>();
        public List<CategoryPerformanceDto> CategoryPerformance { get; set; } = new List<CategoryPerformanceDto>();
        public CustomerInsightsDto CustomerInsights { get; set; } = new CustomerInsightsDto();

        // Additional properties for dashboard display
        public string CurrentTimeRange { get; set; } = "Last 30 days";
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

        // Calculated properties
        public decimal MonthOverMonthGrowth => Stats.TotalRevenue > 0 && Stats.MonthlyRevenue > 0
            ? ((Stats.MonthlyRevenue / Math.Max(Stats.TotalRevenue - Stats.MonthlyRevenue, 1)) * 100)
            : 0;

        public int TotalActiveCustomers => CustomerInsights.NewCustomers + CustomerInsights.ReturningCustomers;

        public double CustomerRetentionRate => Stats.TotalCustomers > 0
            ? (double)CustomerInsights.ReturningCustomers / Stats.TotalCustomers * 100
            : 0;

        public bool HasLowStockAlert => Stats.LowStockProducts > 0;
        public bool HasPendingOrders => Stats.PendingOrders > 0;
        public bool HasNewCustomers => CustomerInsights.NewCustomers > 0;
    }
}
