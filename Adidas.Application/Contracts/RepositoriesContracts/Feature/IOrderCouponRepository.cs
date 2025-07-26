using Adidas.Models.Feature;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adidas.Application.Contracts.RepositoriesContracts.Feature
{
    public interface IOrderCouponRepository : IGenericRepository<OrderCoupon>
    {
        Task<IEnumerable<OrderCoupon>> GetByOrderIdAsync(Guid orderId);
        Task<OrderCoupon?> GetByOrderAndCouponIdAsync(Guid orderId, Guid couponId);
        Task<decimal> GetTotalDiscountAppliedByCouponAsync(Guid couponId);
        Task<int> GetCouponUsageCountAsync(Guid couponId);
        Task<IEnumerable<OrderCoupon>> GetWithIncludesAsync();
    }
}
