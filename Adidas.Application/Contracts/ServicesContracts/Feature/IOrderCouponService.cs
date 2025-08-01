
using Adidas.DTOs.Feature.OrderCouponDTOs;
using Adidas.Models.Feature;
using OrderCouponDto = Adidas.DTOs.Operation.OrderDTOs.Result.OrderCouponDto;

namespace Adidas.Application.Contracts.ServicesContracts.Feature
{
    public interface IOrderCouponService
    {
        Task<IEnumerable<OrderCouponDto>> GetByOrderIdAsync(Guid orderId);
        Task<OrderCouponDto?> GetByOrderAndCouponIdAsync(Guid orderId, Guid couponId);
        Task<decimal> GetTotalDiscountAppliedByCouponAsync(Guid couponId);
        Task<int> GetCouponUsageCountAsync(Guid couponId);
        Task<IEnumerable<OrderCouponDto>> GetWithIncludesAsync();
    }
}
