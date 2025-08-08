using System.Data.Entity;
using Adidas.DTOs.Common_DTOs;
using Mapster;

namespace Adidas.Infra.Operation
{
    public class OrderItemRepository : GenericRepository<OrderItem>, IOrderItemRepository
    {
        public OrderItemRepository(AdidasDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<OrderItem>> GetItemsByOrderIdAsync(Guid orderId)
        {
            return await GetAll((q) =>
            {
                return q.Where(oi => oi.OrderId == orderId && !oi.IsDeleted).Include(oi => oi.Variant)
                    .Include(oi => oi.Order);
            }).ToListAsync();
        }

        public async Task<IEnumerable<OrderItem>> GetItemsByVariantIdAsync(Guid variantId)
        {
            return await GetAll(q => q.Where(oi => oi.VariantId == variantId && !oi.IsDeleted)).ToListAsync();
        }

        public async Task<decimal> GetTotalSalesForVariantAsync(Guid variantId)
        {
            var query = GetAll(q => q.Where(oi => oi.VariantId == variantId && !oi.IsDeleted));
            return await query.SumAsync(oi => oi.TotalPrice);
        }

        public async Task<IEnumerable<OrderItem>> GetBestSellingItemsAsync(int count)
        {
            var query = GetAll(q => q.Where(io => !io.IsDeleted));
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
            var query = GetAll(q => q.Where(oi => oi.VariantId == variantId && !oi.IsDeleted));
            return await query.SumAsync(oi => oi.Quantity);
        }

        public async Task<PagedResultDto<OrderItem>> GetOrderItemsPagedAsync(Guid orderId,
            int pageNumber, int pageSize)
        {
            var orderItems = await GetPagedAsync(pageNumber, pageSize,
                q => q.Where(oi => oi.OrderId == orderId && !oi.IsDeleted));

            return orderItems.Adapt<PagedResultDto<OrderItem>>();
        }
    }
}