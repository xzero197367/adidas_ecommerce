using Adidas.Application.Contracts.RepositoriesContracts.Feature;
using Adidas.Application.Contracts.ServicesContracts.Feature;
using AutoMapper;
using Microsoft.Extensions.Logging;
using OrderCouponDto = Adidas.DTOs.Operation.OrderDTOs.Result.OrderCouponDto;

namespace Adidas.Application.Services.Feature
{
    public class OrderCouponService : IOrderCouponService
    {
        private readonly IOrderCouponRepository repository;
        private readonly IMapper mapper;
        private readonly ILogger<OrderCouponService> logger;

        public OrderCouponService(
            IOrderCouponRepository repository,
            IMapper mapper,
            ILogger<OrderCouponService> logger)
        {
            this.repository = repository;
            this.mapper = mapper;
            this.logger = logger;
        }
        
        
        public async Task<IEnumerable<OrderCouponDto>> GetByOrderIdAsync(Guid orderId)
        {
            var orderCoupons = await repository.GetByOrderIdAsync(orderId);
            return mapper.Map<IEnumerable<OrderCouponDto>>(orderCoupons);
        }

        public async Task<OrderCouponDto?> GetByOrderAndCouponIdAsync(Guid orderId, Guid couponId)
        {
            var orderCoupon = await repository.GetByOrderAndCouponIdAsync(orderId, couponId);
            if(orderCoupon == null) return null;
            return mapper.Map<OrderCouponDto>(orderCoupon);
        }
        
        public async Task<decimal> GetTotalDiscountAppliedByCouponAsync(Guid couponId)
        {
            var totalDiscount = await repository.GetTotalDiscountAppliedByCouponAsync(couponId);
            return totalDiscount;
        }
        
        public async Task<int> GetCouponUsageCountAsync(Guid couponId)
        {
            var usageCount = await repository.GetCouponUsageCountAsync(couponId);
            return usageCount;
        }
        
        public async Task<IEnumerable<OrderCouponDto>> GetWithIncludesAsync()
        {
            var orderCoupons = await repository.GetWithIncludesAsync();
            return mapper.Map<IEnumerable<OrderCouponDto>>(orderCoupons);
        }
     
    }
}