using Adidas.Application.Contracts.RepositoriesContracts.Feature;
using Adidas.Application.Contracts.RepositoriesContracts.Operation;
using Adidas.Application.Contracts.RepositoriesContracts.Separator;
using Adidas.Application.Contracts.ServicesContracts.Feature;
using Adidas.DTOs.Common_DTOs;
using Adidas.DTOs.Feature.CouponDTOs;
using AutoMapper;
using Microsoft.Extensions.Logging;
using Models.Feature;

namespace Adidas.Application.Services.Feature
{
    public class CouponService : GenericService<Coupon, CouponDto, CouponCreateDto, CouponUpdateDto>, ICouponService
    {
        private readonly ICouponRepository _couponRepository;
        // private readonly IOrderCouponRepository _orderCouponRepository;
        private readonly ILogger<CouponService> _logger;

        public CouponService(
            // IOrderCouponRepository orderCouponRepository,
            ICouponRepository couponRepository,
            IMapper mapper,
            ILogger<CouponService> logger) 
            : base(couponRepository, mapper, logger)
        {
            _couponRepository = couponRepository;
            // _orderCouponRepository = orderCouponRepository;
            _logger = logger;
        }
        public async Task<List<CouponDto>> GetAllCouponsAsync()
        {
            var coupons = await _couponRepository.GetAllAsync();

            var result = coupons.Select(c => new CouponDto
            {
                Id = c.Id,
                Code = c.Code,
                Name = c.Name,
                DiscountType = c.DiscountType,
                DiscountValue = c.DiscountValue,
                MinimumAmount = c.MinimumAmount,
                ValidFrom = c.ValidFrom,
                ValidTo = c.ValidTo,
                UsageLimit = c.UsageLimit,
                UsedCount = c.UsedCount,
                IsActive = c.IsActive,
            }).ToList();

            return result;
        }

        public async Task<CouponDto?> GetCouponByCodeAsync(string code)
        {
            try
            {
                var coupon = await _couponRepository.GetByCodeAsync(code);
                return coupon != null ? _mapper.Map<CouponDto>(coupon) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting coupon by code: {Code}", code);
                return null;
            }
        }

        public async Task<IEnumerable<CouponDto>> GetActiveCouponsAsync()
        {
            try
            {
                var coupons = await _couponRepository.GetActiveCouponsAsync();
                return _mapper.Map<IEnumerable<CouponDto>>(coupons);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting active coupons");
                return null;
            }
        }

        public async Task<CouponValidationResultDto> ValidateCouponAsync(string code, decimal orderAmount)
        {
            try
            {
                var coupon = await _couponRepository.GetByCodeAsync(code);
                
                if (coupon == null || coupon.IsDeleted)
                {
                    return CouponValidationResultDto.Failure("Invalid coupon code");
                }

                var now = DateTime.UtcNow;
                if (now < coupon.ValidFrom || now > coupon.ValidTo)
                {
                    return CouponValidationResultDto.Failure("Coupon is not valid at this time");
                }

                if (coupon.UsageLimit > 0 && coupon.UsedCount >= coupon.UsageLimit)
                {
                    return CouponValidationResultDto.Failure("Coupon has reached its usage limit");
                }

                if (orderAmount < coupon.MinimumAmount)
                {
                    return CouponValidationResultDto.Failure(
                        $"Minimum order amount of {coupon.MinimumAmount} is required");
                }

                var discountAmount = await CalculateDiscountAmountAsync(coupon, orderAmount);
                var finalAmount = orderAmount - discountAmount;

                return CouponValidationResultDto.Success(
                    coupon.Code,
                    coupon.Name,
                    coupon.DiscountValue,
                    coupon.DiscountType.ToString(),
                    discountAmount,
                    orderAmount,
                    finalAmount
                );
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

                return await CalculateDiscountAmountAsync(coupon, orderAmount);
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

        private async Task<decimal> CalculateDiscountAmountAsync(Coupon coupon, decimal orderAmount)
        {
            if (coupon.DiscountType == DiscountType.Percentage)
            {
                return orderAmount * (coupon.DiscountValue / 100m);
            }
            else 
            {
                return Math.Min(coupon.DiscountValue, orderAmount);
            }
        }

        public async Task<CouponListResult> GetFilteredPagedCouponsAsync(string search, string status, int page, int pageSize)
        {
            var allCoupons = await GetFilteredCouponsAsync(search, status);
            var list = allCoupons.ToList();

            var result = new CouponListResult
            {
                TotalCount = list.Count,
                ActiveCount = list.Count(c => c.IsValidNow && c.IsActive),
                ExpiredCount = list.Count(c => c.IsExpired),
                TotalUsage = list.Sum(c => c.UsedCount),
                TotalSavings = CalculateTotalSavings(list),
                Coupons = list
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList()
            };

            return result;
        }

        public async Task<IEnumerable<CouponDto>> GetFilteredCouponsAsync(string search, string status)
        {
            var allCoupons = await _couponRepository.GetAllAsync();
            allCoupons = allCoupons.Where(c=>!c.IsDeleted);
            if (!string.IsNullOrWhiteSpace(search))
            {
                allCoupons = allCoupons
                    .Where(c => c.Code.Contains(search, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(status) && status.ToLower() != "all")
            {
                switch (status.ToLower())
                {
                    case "active":
                        allCoupons = allCoupons.Where(c =>
                            c.IsActive &&
                            !c.IsDeleted &&
                            c.ValidFrom <= DateTime.UtcNow &&
                            c.ValidTo >= DateTime.UtcNow);
                        break;

                    case "expired":
                        allCoupons = allCoupons.Where(c =>
                            c.ValidTo < DateTime.UtcNow);
                        break;

                    case "inactive":
                        allCoupons = allCoupons.Where(c =>
                            c.IsActive && !c.IsDeleted);
                        break;
                }
            }

            var dtoList = allCoupons.Select(c => new CouponDto
            {
                Id = c.Id,
                //CreatedAt = c.CreatedAt,
                UpdatedAt = c.UpdatedAt,
                IsActive = c.IsActive,
                Code = c.Code,
                Name = c.Name,
                DiscountType = c.DiscountType,
                DiscountValue = c.DiscountValue,
                MinimumAmount = c.MinimumAmount,
                ValidFrom = c.ValidFrom,
                ValidTo = c.ValidTo,
                UsageLimit = c.UsageLimit,
                UsedCount = c.UsedCount,
            });

            return dtoList;
        }

        public async Task<Result> SoftDeletAsync(Guid id)
        {
            var coupon = await _couponRepository.GetByIdAsync(id);
         
            if (coupon == null)
                return Result.Failure("coupon not found.");
               if (coupon.IsDeleted)
                return Result.Failure("Coupon is already deleted.");


            await _couponRepository.SoftDeleteAsync(id);
            var result = await _couponRepository.SaveChangesAsync();
          
            return result < 1
                ? Result.Failure("Failed to delete coupon.")
                : Result.Success();

        }


        private decimal CalculateTotalSavings(IEnumerable<CouponDto> coupons)
        {
            decimal total = 0;

            foreach (var coupon in coupons)
            {
                if (coupon.UsedCount > 0)
                {
                    switch (coupon.DiscountType)
                    {
                        case DiscountType.Amount:
                        case DiscountType.FixedAmount:
                            total += coupon.DiscountValue * coupon.UsedCount;
                            break;

                        case DiscountType.Percentage:
                            total += (coupon.MinimumAmount * (coupon.DiscountValue / 100m)) * coupon.UsedCount;
                            break;
                    }
                }
            }

            return total;
        }

    

    }
}