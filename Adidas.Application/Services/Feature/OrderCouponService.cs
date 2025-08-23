//using Adidas.Application.Contracts.RepositoriesContracts.Feature;
//using Adidas.Application.Contracts.ServicesContracts.Feature;
//using Adidas.DTOs.Common_DTOs;
//using Adidas.DTOs.CommonDTOs;
//using Adidas.DTOs.Feature.OrderCouponDTOs;
//using Adidas.Models.Feature;
//using Mapster;
//using Microsoft.Extensions.Logging;

//namespace Adidas.Application.Services.Feature
//{
//    public class OrderCouponService : GenericService<OrderCoupon, OrderCouponDto, OrderCouponCreateDto, OrderCouponUpdateDto>,IOrderCouponService
//    {
//        private readonly IOrderCouponRepository repository;
//        private readonly ILogger<OrderCouponService> logger;

//        public OrderCouponService(
//            IOrderCouponRepository repository,
//            ILogger<OrderCouponService> logger): base(repository, logger)
//        {
//            this.repository = repository;
//            this.logger = logger;
//        }


//        public async Task<OperationResult<IEnumerable<OrderCouponDto>>> GetByOrderIdAsync(Guid orderId)
//        {
//            try
//            {
//                var orderCoupons = await repository.GetByOrderIdAsync(orderId);
//                return OperationResult<IEnumerable<OrderCouponDto>>.Success(orderCoupons
//                    .Adapt<IEnumerable<OrderCouponDto>>());
//            }
//            catch (Exception ex)
//            {
//                logger.LogError(ex, "Error getting entity by id {Id} with includes", orderId);
//                return OperationResult<IEnumerable<OrderCouponDto>>.Fail("Error getting entity by id");
//            }
//        }

//        public async Task<OperationResult<OrderCouponDto>> GetByOrderAndCouponIdAsync(Guid orderId, Guid couponId)
//        {
//            try
//            {
//                var orderCoupon = await repository.GetByOrderAndCouponIdAsync(orderId, couponId);
//                if (orderCoupon == null) return OperationResult<OrderCouponDto>.Fail("OrderCoupon not found");
//                return OperationResult<OrderCouponDto>.Success(orderCoupon.Adapt<OrderCouponDto>());
//            }
//            catch (Exception ex)
//            {
//                logger.LogError(ex, "Error getting entity by id {Id} with includes", orderId);
//                return OperationResult<OrderCouponDto>.Fail("Error getting entity by id");
//            }
//        }

//        public async Task<OperationResult<decimal>> GetTotalDiscountAppliedByCouponAsync(Guid couponId)
//        {
//            try
//            {
//                var totalDiscount = await repository.GetTotalDiscountAppliedByCouponAsync(couponId);
//                return OperationResult<decimal>.Success(totalDiscount);
//            }catch (Exception ex)
//            {
//                logger.LogError(ex, "Error getting entity by id {Id} with includes", couponId);
//                return OperationResult<decimal>.Fail("Error getting entity by id");
//            }
//        }

//        public async Task<OperationResult<int>> GetCouponUsageCountAsync(Guid couponId)
//        {
//            try
//            {
//                var usageCount = await repository.GetCouponUsageCountAsync(couponId);
//                return OperationResult<int>.Success(usageCount);
//            }catch (Exception ex)
//            {
//                logger.LogError(ex, "Error getting entity by id {Id} with includes", couponId);
//                return OperationResult<int>.Fail("Error getting entity by id");
//            }
//        }

//        public async Task<OperationResult<IEnumerable<OrderCouponDto>>> GetWithIncludesAsync()
//        {
//            try
//            {
//                var orderCoupons = await repository.GetWithIncludesAsync();
//                return OperationResult<IEnumerable<OrderCouponDto>>.Success(orderCoupons
//                    .Adapt<IEnumerable<OrderCouponDto>>());
//            }
//            catch (Exception ex)
//            {
//                logger.LogError(ex, "Error getting entity by id {Id} with includes");
//                return OperationResult<IEnumerable<OrderCouponDto>>.Fail("Error getting entity by id");
//            }
//        }
//    }
//}
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
    public class OrderCouponService : GenericService<OrderCoupon, OrderCouponDto, OrderCouponCreateDto, OrderCouponUpdateDto>, IOrderCouponService
    {
        private readonly IOrderCouponRepository repository;
        private readonly ILogger<OrderCouponService> logger;

        public OrderCouponService(
            IOrderCouponRepository repository,
            ILogger<OrderCouponService> logger) : base(repository, logger)
        {
            this.repository = repository;
            this.logger = logger;
        }

        public async Task<OperationResult<IEnumerable<OrderCouponDto>>> GetByOrderIdAsync(Guid orderId)
        {
            try
            {
                var orderCoupons = await repository.GetByOrderIdAsync(orderId);

                // Manual mapping to avoid casting issues
                var dtos = orderCoupons.Select(oc => new OrderCouponDto
                {
                    Id = oc.Id,
                    UpdatedAt = oc.UpdatedAt,
                    IsActive = oc.IsActive,
                    DiscountApplied = oc.DiscountApplied,
                    CouponId = oc.CouponId,
                    OrderId = oc.OrderId
                    // Don't include navigation properties here to avoid casting issues
                }).ToList();

                return OperationResult<IEnumerable<OrderCouponDto>>.Success(dtos);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error getting order coupons by order id {OrderId}", orderId);
                return OperationResult<IEnumerable<OrderCouponDto>>.Fail("Error getting order coupons by order id");
            }
        }

        public async Task<OperationResult<OrderCouponDto>> GetByOrderAndCouponIdAsync(Guid orderId, Guid couponId)
        {
            try
            {
                var orderCoupon = await repository.GetByOrderAndCouponIdAsync(orderId, couponId);
                if (orderCoupon == null)
                    return OperationResult<OrderCouponDto>.Fail("OrderCoupon not found");

                // Manual mapping
                var dto = new OrderCouponDto
                {
                    Id = orderCoupon.Id,
                    UpdatedAt = orderCoupon.UpdatedAt,
                    IsActive = orderCoupon.IsActive,
                    DiscountApplied = orderCoupon.DiscountApplied,
                    CouponId = orderCoupon.CouponId,
                    OrderId = orderCoupon.OrderId
                };

                return OperationResult<OrderCouponDto>.Success(dto);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error getting order coupon by order id {OrderId} and coupon id {CouponId}", orderId, couponId);
                return OperationResult<OrderCouponDto>.Fail("Error getting order coupon");
            }
        }

        public async Task<OperationResult<decimal>> GetTotalDiscountAppliedByCouponAsync(Guid couponId)
        {
            try
            {
                var totalDiscount = await repository.GetTotalDiscountAppliedByCouponAsync(couponId);
                return OperationResult<decimal>.Success(totalDiscount);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error getting total discount by coupon id {CouponId}", couponId);
                return OperationResult<decimal>.Fail("Error getting total discount");
            }
        }

        public async Task<OperationResult<int>> GetCouponUsageCountAsync(Guid couponId)
        {
            try
            {
                var usageCount = await repository.GetCouponUsageCountAsync(couponId);
                return OperationResult<int>.Success(usageCount);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error getting coupon usage count for coupon id {CouponId}", couponId);
                return OperationResult<int>.Fail("Error getting coupon usage count");
            }
        }

        public async Task<OperationResult<IEnumerable<OrderCouponDto>>> GetWithIncludesAsync()
        {
            try
            {
                var orderCoupons = await repository.GetWithIncludesAsync();

                // Safe manual mapping with null checks
                var dtos = orderCoupons.Select(oc => new OrderCouponDto
                {
                    Id = oc.Id,
                    UpdatedAt = oc.UpdatedAt,
                    IsActive = oc.IsActive,
                    DiscountApplied = oc.DiscountApplied,
                    CouponId = oc.CouponId,
                    OrderId = oc.OrderId,
                    // Only include navigation properties if they exist and are not null
                    Coupon = oc.Coupon != null ? new Adidas.DTOs.Feature.CouponDTOs.CouponDto
                    {
                        Id = oc.Coupon.Id,
                        UpdatedAt = oc.Coupon.UpdatedAt,
                        IsActive = oc.Coupon.IsActive,
                        Code = oc.Coupon.Code,
                        Name = oc.Coupon.Name,
                        DiscountType = oc.Coupon.DiscountType,
                        DiscountValue = oc.Coupon.DiscountValue,
                        MinimumAmount = oc.Coupon.MinimumAmount,
                        ValidFrom = oc.Coupon.ValidFrom,
                        ValidTo = oc.Coupon.ValidTo,
                        UsageLimit = oc.Coupon.UsageLimit,
                        UsedCount = oc.Coupon.UsedCount,
                        IsValidNow = DateTime.Now >= oc.Coupon.ValidFrom && DateTime.Now <= oc.Coupon.ValidTo,
                        IsExpired = DateTime.Now > oc.Coupon.ValidTo,
                        StatusText = string.Empty,
                        DiscountDisplayText = string.Empty
                    } : null
                    // Note: Excluding Order navigation to avoid potential JSON serialization issues
                }).ToList();

                return OperationResult<IEnumerable<OrderCouponDto>>.Success(dtos);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error getting order coupons with includes");
                return OperationResult<IEnumerable<OrderCouponDto>>.Fail("Error getting order coupons with includes");
            }
        }
    }
}