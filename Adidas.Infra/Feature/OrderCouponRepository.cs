using Adidas.Application.Contracts.RepositoriesContracts.Feature;
using Adidas.Context;
using Adidas.Models.Feature;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adidas.Infra.Feature
{
    public class OrderCouponRepository : GenericRepository<OrderCoupon>, IOrderCouponRepository
    {
        public OrderCouponRepository(AdidasDbContext context) : base(context) { }

        public async Task<IEnumerable<OrderCoupon>> GetByOrderIdAsync(Guid orderId)
        {
            return await _dbSet
                .Where(oc => oc.OrderId == orderId)
                .ToListAsync();
        }

        public async Task<OrderCoupon?> GetByOrderAndCouponIdAsync(Guid orderId, Guid couponId)
        {
            return await _dbSet
                .FirstOrDefaultAsync(oc => oc.OrderId == orderId && oc.CouponId == couponId);
        }

        public async Task<decimal> GetTotalDiscountAppliedByCouponAsync(Guid couponId)
        {
            return await _dbSet
                .Where(oc => oc.CouponId == couponId)
                .SumAsync(oc => oc.DiscountApplied);
        }

        public async Task<int> GetCouponUsageCountAsync(Guid couponId)
        {
            return await _dbSet
                .CountAsync(oc => oc.CouponId == couponId);
        }

        public async Task<IEnumerable<OrderCoupon>> GetWithIncludesAsync()
        {
            return await _dbSet
                .Include(oc => oc.Order)
                .Include(oc => oc.Coupon)
                .ToListAsync();
        }
    }
}