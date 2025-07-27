
using System.Data.Entity;

namespace Adidas.Infra.Operation
{
    public class OrderItemRepository : GenericRepository<OrderItem>, IOrderItemRepository
    {
        public OrderItemRepository(AdidasDbContext context) : base(context) { }

        public async Task<IEnumerable<OrderItem>> GetItemsByOrderIdAsync(Guid orderId)
        {
            return await FindAsync(oi => oi.OrderId == orderId && !oi.IsDeleted,
                                 oi => oi.Variant,
                                 oi => oi.Order);
        }

        public async Task<IEnumerable<OrderItem>> GetItemsByVariantIdAsync(Guid variantId)
        {
            return await FindAsync(oi => oi.VariantId == variantId && !oi.IsDeleted,
                                 oi => oi.Order);
        }

        public async Task<decimal> GetTotalSalesForVariantAsync(Guid variantId)
        {
            var query = GetQueryable(oi => oi.VariantId == variantId && !oi.IsDeleted);
            return await query.SumAsync(oi => oi.TotalPrice);
        }

        public async Task<IEnumerable<OrderItem>> GetBestSellingItemsAsync(int count)  
        {
            var query = GetQueryable(oi => !oi.IsDeleted);
            return await query
                .GroupBy(oi => oi.VariantId)
                .OrderByDescending(g => g.Sum(oi => oi.Quantity))
                .Take(count)
                .SelectMany(g => g)
                .Include(oi => oi.Variant)
                .ToListAsync();
        }

        public async Task<int> GetTotalQuantitySoldAsync(Guid variantId)
        {
            var query = GetQueryable(oi => oi.VariantId == variantId && !oi.IsDeleted);
            return await query.SumAsync(oi => oi.Quantity);
        }

        public async Task<(IEnumerable<OrderItem> items, int totalCount)> GetOrderItemsPagedAsync(Guid orderId, int pageNumber, int pageSize)
        {
            return await GetPagedAsync(pageNumber, pageSize, oi => oi.OrderId == orderId && !oi.IsDeleted);
        }
    }
}
