using Adidas.Application.Contracts.RepositoriesContracts.Feature;
using Adidas.Application.Contracts.RepositoriesContracts.Operation;
using Adidas.Application.Contracts.RepositoriesContracts.Separator;
using Adidas.Application.Contracts.ServicesContracts.Feature;
using Adidas.DTOs.Common_DTOs;
using Adidas.DTOs.Feature.CouponDTOs;
using Adidas.DTOs.Feature.OrderCouponDTOs;
using Adidas.Models.Feature;
using Adidas.Models.Separator;
using Microsoft.Extensions.Logging;
using Models.Feature;
using System.Collections.Generic;

namespace Adidas.Application.Services.Feature
{
    public class CouponService : ICouponService
    {
        private readonly ICouponRepository _couponRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly IOrderCouponRepository _orderCouponRepository;
        public CouponService(
            ICouponRepository couponRepository, IOrderRepository orderRepository, IOrderCouponRepository orderCouponRepository)

        {
            _couponRepository = couponRepository;
            _orderRepository = orderRepository;
            _orderCouponRepository = orderCouponRepository;
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

        private async Task<IEnumerable<CouponDto>> GetFilteredCouponsAsync(string search, string status)
        {
            var allCoupons = await _couponRepository.GetAllAsync();
            allCoupons = allCoupons.Where(c => !c.IsDeleted);
            if (!string.IsNullOrWhiteSpace(search))
            {
                allCoupons = allCoupons
                    .Where(c => c.Code.Contains(search, StringComparison.OrdinalIgnoreCase));
            }
            var now = DateTime.Now;

            if (!string.IsNullOrWhiteSpace(status) && status.ToLower() != "all")
            {
                switch (status.ToLower())
                {
                    case "active":
                        allCoupons = allCoupons.Where(c =>
                            c.IsActive &&
                            !c.IsDeleted &&
                            c.ValidFrom <= now &&
                            c.ValidTo >= now);
                        break;

                    case "expired":
                        allCoupons = allCoupons.Where(c =>
                            c.ValidTo < now);
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
                IsValidNow = now >= c.ValidFrom && now <= c.ValidTo,
                IsExpired = now > c.ValidTo
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

        private decimal CalculateCouponSavings(CouponDto coupon)
        {
            decimal total = 0;
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
            return total;
        }

        private decimal CalculateTotalSavings(IEnumerable<CouponDto> coupons)
        {
            decimal total = 0;

            foreach (var coupon in coupons)
            {
                total += CalculateCouponSavings(coupon);
            }

            return total;
        }

        public async Task<Result> ToggleCouponStatusAsync(Guid id)
        {
            var coupon = await _couponRepository.GetByIdAsync(id);
            if (coupon == null)
                return Result.Failure("coupon not found.");

            var now = DateTime.Now;

            if (now < coupon.ValidFrom || now > coupon.ValidTo)
            {
                return Result.Failure("Coupon is not valid at this time.");
            }

            coupon.IsActive = !coupon.IsActive;

            try
            {

                await _couponRepository.UpdateAsync(coupon);
                var result = await _couponRepository.SaveChangesAsync();

                if (result > 0)
                    return Result.Success();
                else
                    return Result.Failure("No changes were made to the database.");
            }
            catch (Exception ex)
            {

                return Result.Failure("An error occurred while updating the coupon status.");
            }

        }

        public async Task<Result> UpdateAsync(CouponUpdateDto dto)
        {

            var coupon = await _couponRepository.GetByIdAsync(dto.Id);
            if (coupon == null || coupon.IsDeleted)
            {
                return Result.Failure("Coupon not found.");
            }


            var otherCoupon = await _couponRepository.GetByCodeAsync(dto.Code);
            if (otherCoupon != null && otherCoupon.Id != dto.Id)
            {
                return Result.Failure($"Coupon code '{dto.Code}' is already in use.");
            }


            coupon.Code = dto.Code;
            coupon.Name = dto.Name;
            coupon.DiscountType = dto.DiscountType;
            coupon.DiscountValue = dto.DiscountValue;
            coupon.MinimumAmount = dto.MinimumAmount;
            coupon.ValidFrom = dto.ValidFrom;
            coupon.ValidTo = dto.ValidTo;
            coupon.UsageLimit = dto.UsageLimit;
            //coupon.IsActive = dto.IsActive;
            //coupon.UpdatedAt = DateTime.UtcNow;

            try
            {

                await _couponRepository.UpdateAsync(coupon);
                var affectedRows = await _couponRepository.SaveChangesAsync();

                if (affectedRows <= 0)
                {
                    return Result.Failure("No changes were saved for the coupon.");
                }

                return Result.Success();
            }
            catch (Exception ex)
            {
                return Result.Failure($"Could not update the coupon: {ex.Message}");
            }
        }


        public async Task<Result> CreateAsync(CouponCreateDto dto)
        {
            var couponExist = await _couponRepository.GetByCodeAsync(dto.Code);

            if (couponExist != null)
            {
                return Result.Failure($"Coupon code '{dto.Code}' already exists.");
            }

            var coupon = new Coupon
            {
                Code = dto.Code,
                Name = dto.Name,
                DiscountType = dto.DiscountType,
                DiscountValue = dto.DiscountValue,
                MinimumAmount = dto.MinimumAmount,
                ValidFrom = dto.ValidFrom,
                ValidTo = dto.ValidTo,
                UsageLimit = dto.UsageLimit,
                UsedCount = 0,
                IsActive = dto.IsActive,
                //CreatedAt = DateTime.UtcNow,
                //UpdatedAt = DateTime.UtcNow
            };

            try
            {

                await _couponRepository.AddAsync(coupon);
                var affectedRows = await _couponRepository.SaveChangesAsync();

                if (affectedRows <= 0)
                {
                    return Result.Failure("No changes were made when adding the coupon.");
                }

                return Result.Success();
            }
            catch (Exception ex)
            {
                return Result.Failure($"Could not add the coupon: {ex.Message}");
            }
        }

        public async Task<CouponUpdateDto> GetCouponToEditByIdAsync(Guid id)
        {
            var coupon = await _couponRepository.GetByIdAsync(id);
            if (coupon == null)
                return null;
            var couponUpdateDto = new CouponUpdateDto
            {
                Code = coupon.Code,
                Name = coupon.Name,
                DiscountType = coupon.DiscountType,
                DiscountValue = coupon.DiscountValue,
                MinimumAmount = coupon.MinimumAmount,
                ValidFrom = coupon.ValidFrom,
                ValidTo = coupon.ValidTo,
                UsageLimit = coupon.UsageLimit,
                //IsActive = coupon.IsActive,

            };

            return couponUpdateDto;
        }



        public async Task<CouponDetailsDTO> GetCouponDetailsByIdAsync(Guid id)
        {
            var coupon = await _couponRepository.GetByIdAsync(id, c => c.OrderCoupons);
            var now = DateTime.Now;
            if (coupon == null)
                return new CouponDetailsDTO();

            var orderCoupons = coupon.OrderCoupons;

            string statusText;
            if (coupon.UsedCount >= coupon.UsageLimit)
                statusText = "Limit Reached";
            else if (now > coupon.ValidTo)
                statusText = "Expired";
            else if (coupon.IsActive && now >= coupon.ValidFrom && now <= coupon.ValidTo)
                statusText = "Active";
            else
                statusText = "Inactive";

            var couponDto = new CouponDto
            {
                Id = coupon.Id,
                UpdatedAt = coupon.UpdatedAt,
                IsActive = coupon.IsActive,
                Code = coupon.Code,
                Name = coupon.Name,
                DiscountType = coupon.DiscountType,
                DiscountValue = coupon.DiscountValue,
                MinimumAmount = coupon.MinimumAmount,
                ValidFrom = coupon.ValidFrom,
                ValidTo = coupon.ValidTo,
                UsageLimit = coupon.UsageLimit,
                UsedCount = coupon.UsedCount,
                IsValidNow = now >= coupon.ValidFrom && now <= coupon.ValidTo,
                IsExpired = now > coupon.ValidTo,
                StatusText = statusText
            };

            var orderCouponDtos = orderCoupons.Select(oc => new OrderCouponDto
            {
                Id = oc.Id,
                UpdatedAt = oc.UpdatedAt,
                IsActive = oc.IsActive,
                DiscountApplied = oc.DiscountApplied,
                CouponId = oc.CouponId,
                OrderId = oc.OrderId
            }).ToList();

            var couponDetails = new CouponDetailsDTO
            {
                CouponDto = couponDto,
                orderCouponDtos = orderCouponDtos,
                TotalUsage = couponDto.UsedCount,
                TotalSavings = CalculateCouponSavings(couponDto)
            };

            return couponDetails;
        }

        public async Task<CouponApplicationResult> ApplyCouponToOrderAsync(Guid orderId, string couponCode)
        {
            var order = await _orderRepository.GetByIdAsync(orderId, c => c.OrderCoupons);
            if (order == null)
                return CouponApplicationResult.Fail("Order not found.");

            var coupon = await _couponRepository.GetByCodeAsync(couponCode);

            var validationResult = ValidateCoupon(coupon, order);
            if (!validationResult.Success)
                return validationResult;


            if (order.OrderCoupons.Any(oc => oc.CouponId == coupon.Id))
                return CouponApplicationResult.Fail("Coupon already applied to this order.");

            decimal discountApplied = CalculateDiscount(order.Subtotal, coupon);

            order.DiscountAmount += discountApplied;
            order.TotalAmount = order.Subtotal + order.TaxAmount + order.ShippingAmount - order.DiscountAmount;

            await _orderRepository.UpdateAsync(order);

            await _orderCouponRepository.AddAsync(new OrderCoupon
            {
                CouponId = coupon.Id,
                OrderId = order.Id,
                DiscountApplied = discountApplied,
                AddedById = order.AddedById
            });

            coupon.UsedCount++;
            await _couponRepository.UpdateAsync(coupon);


            await _couponRepository.SaveChangesAsync();

            return CouponApplicationResult.Ok(discountApplied, order.TotalAmount);
        }

        private CouponApplicationResult ValidateCoupon(Coupon coupon, Order order)
        {
            if (coupon == null || !(coupon.IsActive && !coupon.IsDeleted))
                return CouponApplicationResult.Fail("Coupon not found or inactive.");

            var now = DateTime.Now;
            if (now < coupon.ValidFrom || now > coupon.ValidTo)
                return CouponApplicationResult.Fail("Coupon is not valid at this time.");

            if (order.Subtotal < coupon.MinimumAmount)
                return CouponApplicationResult.Fail(
                    $"Order subtotal must be at least {coupon.MinimumAmount:C} to apply this coupon."
                );

            if (coupon.UsageLimit > 0 && coupon.UsedCount >= coupon.UsageLimit)
                return CouponApplicationResult.Fail("Coupon usage limit reached.");

            return CouponApplicationResult.Ok();
        }

        private decimal CalculateDiscount(decimal subtotal, Coupon coupon)
        {
            decimal discount = coupon.DiscountType switch
            {
                DiscountType.Percentage => subtotal * (coupon.DiscountValue / 100m),
                _ => coupon.DiscountValue
            };

            return Math.Min(Math.Round(discount, 2), subtotal);
        }


        public async Task<CouponApplicationResult> CancelCouponApplicationAsync(Guid orderId, string couponCode)
        {
            var order = await _orderRepository.GetByIdAsync(orderId, c => c.OrderCoupons);
            if (order == null)
                return CouponApplicationResult.Fail("Order not found.");

            var coupon = await _couponRepository.GetByCodeAsync(couponCode);
            if (coupon == null)
                return CouponApplicationResult.Fail("Coupon not found.");

            var appliedCoupon = order.OrderCoupons.FirstOrDefault(oc => oc.CouponId == coupon.Id);
            if (appliedCoupon == null)
                return CouponApplicationResult.Fail("Coupon is not applied to this order.");


            order.DiscountAmount -= appliedCoupon.DiscountApplied;
            if (order.DiscountAmount < 0)
                order.DiscountAmount = 0;


            order.TotalAmount = order.Subtotal + order.TaxAmount + order.ShippingAmount - order.DiscountAmount;


            order.OrderCoupons.Remove(appliedCoupon);

            await _orderCouponRepository.HardDeleteAsync(appliedCoupon.Id);

            if (coupon.UsedCount > 0)
                coupon.UsedCount--;

            await _orderRepository.UpdateAsync(order);
            await _couponRepository.UpdateAsync(coupon);
            await _couponRepository.SaveChangesAsync();

            return CouponApplicationResult.Ok(0, order.TotalAmount);
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

                return -1;
            }
        }

    }
}
