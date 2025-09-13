using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using System.Linq;

namespace Adidas.Infra.Operation
{
    public class OrderRepository : GenericRepository<Order>, IOrderRepository
    {
        public OrderRepository(AdidasDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Order>> GetOrdersByUserIdAsync(string userId)
        {
            return await GetAll().Where(o => o.UserId == userId && !o.IsDeleted).Include(o => o.OrderItems)
                        .Include(o => o.Payments)
                .ToListAsync();
        }
        public async Task<Order?> GetOrderByOrderIdAsync(Guid orderId)
        {
            return await _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Variant) // Include product details
                        .ThenInclude(pv => pv.Product)
                .Include(o => o.Payments)
                .FirstOrDefaultAsync(o => o.Id == orderId);
        }
        public async Task<IEnumerable<Order>> GetOrdersByStatusAsync(OrderStatus status)
        {
            return await GetAll().Where(o => o.OrderStatus == status && !o.IsDeleted).ToListAsync();
        }

        public IEnumerable<Order> GetOrdersByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return GetAll().Where(o => o.OrderDate >= startDate &&
                                                  o.OrderDate <= endDate &&
                                                  !o.IsDeleted).ToList();
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
            return await FindAsync(q => q.Where(o => o.OrderNumber == orderNumber && !o.IsDeleted));
        }

        public async Task<IEnumerable<Order>> GetPendingOrdersAsync()
        {
            return await GetAll().Where(o => o.OrderStatus == OrderStatus.Pending && !o.IsDeleted).ToListAsync();
        }

        public decimal GetTotalSalesAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = _context.Orders
                .Where(o => o.OrderStatus == OrderStatus.Delivered && !o.IsDeleted);

            if (startDate.HasValue)
                query = query.Where(o => o.OrderDate >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(o => o.OrderDate <= endDate.Value);

            return query.Sum(o => o.TotalAmount);
        }

        public async Task<(IEnumerable<Order> orders, int totalCount)> GetPagedOrdersAsync(
        int pageNumber, int pageSize,
        Expression<Func<Order, bool>>? filter = null)
        {
            var query = _context.Orders
                .Include(o => o.User)  // ✅ ensures User is loaded
                .Where(o => !o.IsDeleted);

            if (filter != null)
                query = query.Where(filter);

            var totalCount = await query.CountAsync();

            var orders = await query
                .OrderByDescending(o => o.OrderDate)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (orders, totalCount);
        }




        public async Task<(IEnumerable<Order> orders, int totalCount)> GetUserOrderHistoryPagedAsync(string userId,
            int pageNumber, int pageSize)
        {
            var query = _context.Orders
                .Where(o => o.UserId == userId && !o.IsDeleted)
                .OrderByDescending(o => o.OrderDate);

            var totalCount = await query.CountAsync();
            var orders = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();

            return (orders, totalCount);
        }
    }
}