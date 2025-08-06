
using Adidas.DTOs.Common_DTOs;
using Adidas.DTOs.Feature.OrderCouponDTOs;
using Adidas.Models.Feature;

namespace Adidas.Application.Contracts.ServicesContracts.Feature
{
    public interface IOrderCouponService
    {
        Task<OperationResult<IEnumerable<OrderCouponDto>>> GetByOrderIdAsync(Guid orderId);
        Task<OperationResult<OrderCouponDto>> GetByOrderAndCouponIdAsync(Guid orderId, Guid couponId);
        Task<OperationResult<decimal>> GetTotalDiscountAppliedByCouponAsync(Guid couponId);
        Task<OperationResult<int>> GetCouponUsageCountAsync(Guid couponId);
        Task<OperationResult<IEnumerable<OrderCouponDto>>> GetWithIncludesAsync();
    }
}
