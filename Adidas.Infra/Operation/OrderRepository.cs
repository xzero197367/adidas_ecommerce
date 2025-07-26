using System.Data.Entity;

namespace Adidas.Infra.Operation
{
    public class OrderRepository : GenericRepository<Order>, IOrderRepository
    {
        public OrderRepository(AdidasDbContext context) : base(context) { }

        public async Task<IEnumerable<Order>> GetOrdersByUserIdAsync(Guid userId)
        {
            return await FindAsync(o => o.UserId == userId && !o.IsDeleted,
                                 o => o.OrderItems,
                                 o => o.Payments);
        }

        public async Task<IEnumerable<Order>> GetOrdersByStatusAsync(OrderStatus status)
        {
            return await FindAsync(o => o.OrderStatus == status && !o.IsDeleted);
        }

        public async Task<IEnumerable<Order>> GetOrdersByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await FindAsync(o => o.OrderDate >= startDate &&
                                       o.OrderDate <= endDate &&
                                       !o.IsDeleted);
        }

        public async Task<Order?> GetOrderWithItemsAsync(Guid orderId)
        {
            return await GetByIdAsync(orderId,
                                    o => o.OrderItems,
                                    o => o.User);
        }

        public async Task<Order?> GetOrderWithPaymentsAsync(Guid orderId)
        {
            return await GetByIdAsync(orderId, o => o.Payments);
        }

        public async Task<Order?> GetOrderWithCouponsAsync(Guid orderId)
        {
            return await GetByIdAsync(orderId, o => o.OrderCoupons);
        }

        public async Task<Order?> GetOrderByNumberAsync(string orderNumber)
        {
            return await FirstOrDefaultAsync(o => o.OrderNumber == orderNumber && !o.IsDeleted);
        }

        public async Task<IEnumerable<Order>> GetPendingOrdersAsync()
        {
            return await FindAsync(o => o.OrderStatus == OrderStatus.Pending && !o.IsDeleted);
        }

        public async Task<decimal> GetTotalSalesAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = GetQueryable(o => o.OrderStatus == OrderStatus.Delivered && !o.IsDeleted);

            if (startDate.HasValue)
                query = query.Where(o => o.OrderDate >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(o => o.OrderDate <= endDate.Value);

            return await query.SumAsync(o => o.TotalAmount);
        }

        public async Task<(IEnumerable<Order> orders, int totalCount)> GetUserOrderHistoryPagedAsync(Guid userId, int pageNumber, int pageSize)
        {
            return await GetPagedAsync(pageNumber, pageSize, o => o.UserId == userId && !o.IsDeleted);
        }


    }
}
