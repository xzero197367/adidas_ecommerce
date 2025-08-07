
using Adidas.Application.Contracts.RepositoriesContracts.Operation;
using Adidas.Application.Contracts.ServicesContracts.Feature;
using Adidas.DTOs.CommonDTOs;
using Adidas.DTOs.Feature.CouponDTOs;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Models.Feature;

namespace Adidas.Application.Services.Feature
{
    public class CouponService : GenericService<Coupon, CouponDto, CouponCreateDto, CouponUpdateDto>, ICouponService
    {
        private readonly ICouponRepository _couponRepository;
        public readonly ILogger _logger;

        public CouponService(
            // IOrderCouponRepository orderCouponRepository,
            ICouponRepository couponRepository,
            ILogger<CouponService> logger) : base(couponRepository, logger)
        {
            _couponRepository = couponRepository;
            // _orderCouponRepository = orderCouponRepository;
            _logger = logger;
        }


        public async Task<OperationResult<IEnumerable<CouponDto>>> GetAllCouponsAsync()
        {
            var coupons = await _couponRepository.GetAll().ToListAsync();
            return OperationResult<IEnumerable<CouponDto>>.Success(coupons.Adapt<IEnumerable<CouponDto>>());
        }

        public async Task<OperationResult<CouponDto>> GetCouponByCodeAsync(string code)
        {
            try
            {
                var coupon = await _couponRepository.GetByCodeAsync(code);

                if (coupon == null || coupon.IsDeleted)
                {
                    return OperationResult<CouponDto>.Fail("Invalid coupon code");
                }

                return OperationResult<CouponDto>.Success(coupon.Adapt<CouponDto>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting coupon by code: {Code}", code);
                return OperationResult<CouponDto>.Fail("Error getting coupon by code");
            }
        }

        public async Task<OperationResult<IEnumerable<CouponDto>>> GetActiveCouponsAsync()
        {
            try
            {
                var coupons = await _couponRepository.GetActiveCouponsAsync();

                if (coupons == null)
                {
                    return OperationResult<IEnumerable<CouponDto>>.Fail("No active coupons found");
                }

                return OperationResult<IEnumerable<CouponDto>>.Success(coupons.Adapt<IEnumerable<CouponDto>>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting active coupons");
                return OperationResult<IEnumerable<CouponDto>>.Fail("Error getting active coupons");
            }
        }

        public async Task<OperationResult<CouponDto>> ValidateCouponAsync(string code, decimal orderAmount)
        {
            try
            {
                var coupon = await _couponRepository.GetByCodeAsync(code);

                if (coupon == null || coupon.IsDeleted)
                {
                    return OperationResult<CouponDto>.Fail("Invalid coupon code");
                }

                var now = DateTime.UtcNow;
                if (now < coupon.ValidFrom || now > coupon.ValidTo)
                {
                    return OperationResult<CouponDto>.Fail("Coupon is not valid");
                }

                if (coupon.UsageLimit > 0 && coupon.UsedCount >= coupon.UsageLimit)
                {
                    return OperationResult<CouponDto>.Fail("Coupon usage limit reached");
                }

                if (orderAmount < coupon.MinimumAmount)
                {
                    return OperationResult<CouponDto>.Fail("Order amount is less than the minimum amount");
                }

                var discountAmount = CalculateDiscountAmountAsync(coupon, orderAmount);
                var finalAmount = orderAmount - discountAmount;

                if (finalAmount < 0)
                {
                    return OperationResult<CouponDto>.Fail("Order amount is less than the minimum amount");
                }

                return OperationResult<CouponDto>.Success(new CouponDto
                {
                    Code = code,
                    Name = coupon.Name,
                    DiscountValue = coupon.DiscountValue,
                    DiscountType = coupon.DiscountType,
                    DiscountAmount = discountAmount,
                    OriginalAmount = orderAmount,
                    FinalAmount = finalAmount,
                    ValidFrom = coupon.ValidFrom,
                    ValidTo = coupon.ValidTo
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating coupon: {Code}", code);
                return null;
            }
        }

        public async Task<decimal> CalculateCouponAmountAsync(string code, decimal orderAmount)
        {
            try
            {
                var coupon = await _couponRepository.GetByCodeAsync(code);
                if (coupon == null || coupon.IsDeleted)
                {
                    return 0;
                }

                return CalculateDiscountAmountAsync(coupon, orderAmount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating coupon amount for code: {Code}", code);
                return -1;
            }
        }

        public async Task<bool> ApplyCouponAsync(string code)
        {
            try
            {
                var coupon = await _couponRepository.GetByCodeAsync(code);
                if (coupon == null || coupon.IsDeleted)
                {
                    return false;
                }

                if (coupon.UsageLimit > 0 && coupon.UsedCount >= coupon.UsageLimit)
                {
                    return false;
                }

                // TODO: register order coupon and increment usage count

                return await _couponRepository.IncrementUsageCountAsync(coupon.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error applying coupon: {Code}", code);
                return false;
            }
        }

        private decimal CalculateDiscountAmountAsync(Coupon coupon, decimal orderAmount)
        {
            if (coupon.DiscountType == DiscountType.Percentage)
            {
                return orderAmount * (coupon.DiscountValue / 100m);
            }
            else // Fixed amount
            {
                return Math.Min(coupon.DiscountValue, orderAmount);
            }
        }
    }
}