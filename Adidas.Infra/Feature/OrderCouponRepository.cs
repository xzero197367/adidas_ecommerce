//using Adidas.Application.Contracts.RepositoriesContracts.Feature;
//using Adidas.Models.Feature;
//using Microsoft.EntityFrameworkCore;

//namespace Adidas.Infra.Feature
//{
//    public class OrderCouponRepository : GenericRepository<OrderCoupon>, IOrderCouponRepository
//    {
//        public OrderCouponRepository(AdidasDbContext context) : base(context) { }

//        public async Task<IEnumerable<OrderCoupon>> GetByOrderIdAsync(Guid orderId)
//        {
//            return await _dbSet
//                .Where(oc => oc.OrderId == orderId)
//                .ToListAsync();
//        }

//        public async Task<OrderCoupon?> GetByOrderAndCouponIdAsync(Guid orderId, Guid couponId)
//        {
//            return await _dbSet
//                .FirstOrDefaultAsync(oc => oc.OrderId == orderId && oc.CouponId == couponId);
//        }

//        public async Task<decimal> GetTotalDiscountAppliedByCouponAsync(Guid couponId)
//        {
//            return await _dbSet
//                .Where(oc => oc.CouponId == couponId)
//                .SumAsync(oc => oc.DiscountApplied);
//        }

//        public async Task<int> GetCouponUsageCountAsync(Guid couponId)
//        {
//            return await _dbSet
//                .CountAsync(oc => oc.CouponId == couponId);
//        }

//        public async Task<IEnumerable<OrderCoupon>> GetWithIncludesAsync()
//        {
//            return await _dbSet
//                .Include(oc => oc.Order)
//                .Include(oc => oc.Coupon)
//                .ToListAsync();
//        }
//    }
//}
using Adidas.Application.Contracts.RepositoriesContracts.Feature;
using Adidas.Models.Feature;
using Microsoft.EntityFrameworkCore;

namespace Adidas.Infra.Feature
{
    public class OrderCouponRepository : GenericRepository<OrderCoupon>, IOrderCouponRepository
    {
        public OrderCouponRepository(AdidasDbContext context) : base(context) { }

        public async Task<IEnumerable<OrderCoupon>> GetByOrderIdAsync(Guid orderId)
        {
            return await _dbSet
                .Where(oc => oc.OrderId == orderId && !oc.IsDeleted)
                .ToListAsync();
        }

        public async Task<OrderCoupon?> GetByOrderAndCouponIdAsync(Guid orderId, Guid couponId)
        {
            return await _dbSet
                .Where(oc => !oc.IsDeleted)
                .FirstOrDefaultAsync(oc => oc.OrderId == orderId && oc.CouponId == couponId);
        }

        public async Task<decimal> GetTotalDiscountAppliedByCouponAsync(Guid couponId)
        {
            return await _dbSet
                .Where(oc => oc.CouponId == couponId && !oc.IsDeleted)
                .SumAsync(oc => oc.DiscountApplied);
        }

        public async Task<int> GetCouponUsageCountAsync(Guid couponId)
        {
            return await _dbSet
                .Where(oc => !oc.IsDeleted)
                .CountAsync(oc => oc.CouponId == couponId);
        }

        public async Task<IEnumerable<OrderCoupon>> GetWithIncludesAsync()
        {
            try
            {
                return await _dbSet
                    .Where(oc => !oc.IsDeleted)
                    .Include(oc => oc.Order)
                    .Include(oc => oc.Coupon)
                    .AsNoTracking() // Add this to prevent tracking issues
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                // Log the specific error and return without includes as fallback
                // You should inject ILogger here ideally
                return await _dbSet
                    .Where(oc => !oc.IsDeleted)
                    .ToListAsync();
            }
        }
    }
}