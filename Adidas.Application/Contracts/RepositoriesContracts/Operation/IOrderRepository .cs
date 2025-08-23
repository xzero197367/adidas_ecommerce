
namespace Adidas.Application.Contracts.RepositoriesContracts.Operation
{
    public interface IOrderRepository : IGenericRepository<Order>
    {
        Task<IEnumerable<Order>> GetOrdersByUserIdAsync(string userId);
        Task<IEnumerable<Order>> GetOrdersByStatusAsync(OrderStatus status);
        IEnumerable<Order> GetOrdersByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<Order?> GetOrderWithItemsAsync(Guid orderId);
        Task<Order?> GetOrderWithPaymentsAsync(Guid orderId);
        Task<Order?> GetOrderWithCouponsAsync(Guid orderId);
        Task<Order?> GetOrderByNumberAsync(string orderNumber);
        Task<IEnumerable<Order>> GetPendingOrdersAsync();
        decimal GetTotalSalesAsync(DateTime? startDate = null, DateTime? endDate = null);
        Task<(IEnumerable<Order> orders, int totalCount)> GetUserOrderHistoryPagedAsync(string userId, int pageNumber, int pageSize);
        Task<Order?> GetByOrderNumberAsync(string orderNumber);
    }
}
 