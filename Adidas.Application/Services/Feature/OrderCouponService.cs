using Adidas.Application.Contracts.RepositoriesContracts.Feature;
using Adidas.Application.Contracts.ServicesContracts.Feature;
using Adidas.DTOs.Common_DTOs;
using Adidas.DTOs.CommonDTOs;
using Adidas.DTOs.Feature.OrderCouponDTOs;
using Adidas.Models.Feature;
using Mapster;
using Microsoft.Extensions.Logging;

namespace Adidas.Application.Services.Feature
{
    public class OrderCouponService : GenericService<OrderCoupon, OrderCouponDto, OrderCouponCreateDto, OrderCouponUpdateDto>,IOrderCouponService
    {
        private readonly IOrderCouponRepository repository;
        private readonly ILogger<OrderCouponService> logger;

        public OrderCouponService(
            IOrderCouponRepository repository,
            ILogger<OrderCouponService> logger): base(repository, logger)
        {
            this.repository = repository;
            this.logger = logger;
        }


        public async Task<OperationResult<IEnumerable<OrderCouponDto>>> GetByOrderIdAsync(Guid orderId)
        {
            try
            {
                var orderCoupons = await repository.GetByOrderIdAsync(orderId);
                return OperationResult<IEnumerable<OrderCouponDto>>.Success(orderCoupons
                    .Adapt<IEnumerable<OrderCouponDto>>());
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error getting entity by id {Id} with includes", orderId);
                return OperationResult<IEnumerable<OrderCouponDto>>.Fail("Error getting entity by id");
            }
        }

        public async Task<OperationResult<OrderCouponDto>> GetByOrderAndCouponIdAsync(Guid orderId, Guid couponId)
        {
            try
            {
                var orderCoupon = await repository.GetByOrderAndCouponIdAsync(orderId, couponId);
                if (orderCoupon == null) return OperationResult<OrderCouponDto>.Fail("OrderCoupon not found");
                return OperationResult<OrderCouponDto>.Success(orderCoupon.Adapt<OrderCouponDto>());
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error getting entity by id {Id} with includes", orderId);
                return OperationResult<OrderCouponDto>.Fail("Error getting entity by id");
            }
        }

        public async Task<OperationResult<decimal>> GetTotalDiscountAppliedByCouponAsync(Guid couponId)
        {
            try
            {
                var totalDiscount = await repository.GetTotalDiscountAppliedByCouponAsync(couponId);
                return OperationResult<decimal>.Success(totalDiscount);
            }catch (Exception ex)
            {
                logger.LogError(ex, "Error getting entity by id {Id} with includes", couponId);
                return OperationResult<decimal>.Fail("Error getting entity by id");
            }
        }

        public async Task<OperationResult<int>> GetCouponUsageCountAsync(Guid couponId)
        {
            try
            {
                var usageCount = await repository.GetCouponUsageCountAsync(couponId);
                return OperationResult<int>.Success(usageCount);
            }catch (Exception ex)
            {
                logger.LogError(ex, "Error getting entity by id {Id} with includes", couponId);
                return OperationResult<int>.Fail("Error getting entity by id");
            }
        }

        public async Task<OperationResult<IEnumerable<OrderCouponDto>>> GetWithIncludesAsync()
        {
            try
            {
                var orderCoupons = await repository.GetWithIncludesAsync();
                return OperationResult<IEnumerable<OrderCouponDto>>.Success(orderCoupons
                    .Adapt<IEnumerable<OrderCouponDto>>());
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error getting entity by id {Id} with includes");
                return OperationResult<IEnumerable<OrderCouponDto>>.Fail("Error getting entity by id");
            }
        }
    }
}