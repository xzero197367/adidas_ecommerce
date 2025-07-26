
namespace Adidas.Application.Contracts.RepositoriesContracts.Operation
{
    public interface ICouponRepository : IGenericRepository<Coupon>
    {
        Task<Coupon?> GetByCodeAsync(string code);
        Task<IEnumerable<Coupon>> GetActiveCouponsAsync();
        Task<IEnumerable<Coupon>> GetValidCouponsAsync(DateTime date);
        Task<bool> IsCouponValidAsync(string code, decimal orderAmount);
        Task<IEnumerable<Coupon>> GetCouponsByUsageAsync(int maxUsage);
        Task<bool> IncrementUsageCountAsync(Guid couponId);
        Task<(IEnumerable<Coupon> coupons, int totalCount)> GetCouponsPagedAsync(int pageNumber, int pageSize, bool? isActive = null);

        Task<IEnumerable<Coupon>> GetCouponsByUserAsync(string userId);
        Task<bool> DeactivateCouponAsync(Guid couponId);
        Task<IEnumerable<Coupon>> GetExpiredCouponsAsync();
        Task<bool> ApplyCouponToOrderAsync(Guid couponId, Guid orderId);
    }
}
 