
using Adidas.Models.Feature;


namespace Adidas.Infra.Operation
{
    public class CouponRepository : GenericRepository<Coupon>, ICouponRepository
    {
        public CouponRepository(AdidasDbContext context) : base(context) { }

        public async Task<Coupon?> GetByCodeAsync(string code)
        {
            return await FirstOrDefaultAsync(c => c.Code == code && !c.IsDeleted);
        }

        public async Task<IEnumerable<Coupon>> GetActiveCouponsAsync()
        {
            return await FindAsync(c => c.IsActive && !c.IsDeleted);
        }

        public async Task<IEnumerable<Coupon>> GetValidCouponsAsync(DateTime date)
        {
            return await FindAsync(c => c.IsActive &&
                                       !c.IsDeleted &&
                                       c.ValidFrom <= date &&
                                       c.ValidTo >= date);
        }

        public async Task<bool> IsCouponValidAsync(string code, decimal orderAmount)
        {
            var coupon = await FirstOrDefaultAsync(c => c.Code == code &&
                                                       c.IsActive &&
                                                       !c.IsDeleted);

            if (coupon == null) return false;
               
            var now = DateTime.UtcNow;
            return coupon.ValidFrom <= now &&
                   coupon.ValidTo >= now &&
                   orderAmount >= coupon.MinimumAmount &&
                   (coupon.UsageLimit == 0 || coupon.UsedCount < coupon.UsageLimit);
        }

        public async Task<IEnumerable<Coupon>> GetCouponsByUsageAsync(int maxUsage)
        {
            return await FindAsync(c => c.UsedCount <= maxUsage && !c.IsDeleted);
        }

        public async Task<bool> IncrementUsageCountAsync(Guid couponId)
        {
            var coupon = await GetByIdAsync(couponId);
            if (coupon != null && !coupon.IsDeleted)
            {
                coupon.UsedCount++;
                await UpdateAsync(coupon);
                return true;
            }
            return false;
        }

        public async Task<(IEnumerable<Coupon> coupons, int totalCount)> GetCouponsPagedAsync(int pageNumber, int pageSize, bool? isActive = null)
        {
            if (isActive.HasValue)
            {
                return await GetPagedAsync(pageNumber, pageSize, c => c.IsActive == isActive.Value && !c.IsDeleted);
            }
            return await GetPagedAsync(pageNumber, pageSize, c => !c.IsDeleted);
        }

        public async Task<IEnumerable<Coupon>> GetCouponsByUserAsync(string userId)
        {
            return await FindAsync(c => c.AddedById == userId && !c.IsDeleted);
        }

        public async Task<bool> DeactivateCouponAsync(Guid couponId)
        {
            var coupon = await GetByIdAsync(couponId);
            if (coupon == null || coupon.IsDeleted) return false;

            coupon.IsActive = false;
            await UpdateAsync(coupon);
            return true;
        }

        public async Task<IEnumerable<Coupon>> GetExpiredCouponsAsync()
        {
            var now = DateTime.UtcNow;
            return await FindAsync(c => c.ValidTo < now && !c.IsDeleted);
        }

        public async Task<bool> ApplyCouponToOrderAsync(Guid couponId, Guid orderId)
        {
            var coupon = await GetByIdAsync(couponId);
            if (coupon == null || coupon.IsDeleted) return false;

            var orderCoupon = new OrderCoupon
            {
                Id = Guid.NewGuid(),
                CouponId = couponId,
                OrderId = orderId
            };

            await _context.Set<OrderCoupon>().AddAsync(orderCoupon);
            await _context.SaveChangesAsync();

            return true;
        }
    }
}
