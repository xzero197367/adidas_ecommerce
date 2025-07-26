
namespace Adidas.Application.Contracts.RepositoriesContracts.Operation
{
    public interface IOrderRepository : IGenericRepository<Order>
    {
        Task<IEnumerable<Order>> GetOrdersByUserIdAsync(Guid userId);
        Task<IEnumerable<Order>> GetOrdersByStatusAsync(OrderStatus status);
        Task<IEnumerable<Order>> GetOrdersByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<Order?> GetOrderWithItemsAsync(Guid orderId);
        Task<Order?> GetOrderWithPaymentsAsync(Guid orderId);
        Task<Order?> GetOrderWithCouponsAsync(Guid orderId);
        Task<Order?> GetOrderByNumberAsync(string orderNumber);
        Task<IEnumerable<Order>> GetPendingOrdersAsync();
        Task<decimal> GetTotalSalesAsync(DateTime? startDate = null, DateTime? endDate = null);
        Task<(IEnumerable<Order> orders, int totalCount)> GetUserOrderHistoryPagedAsync(Guid userId, int pageNumber, int pageSize);
    }
}
