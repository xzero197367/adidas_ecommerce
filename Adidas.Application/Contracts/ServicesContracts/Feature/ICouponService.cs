using Adidas.Application.Contracts.ServicesContracts;
using Adidas.DTOs.Feature.CouponDTOs;
using Adidas.Models.Feature;
using Models.Feature;

namespace Adidas.Application.Contracts.ServicesContracts.Feature
{
    public interface ICouponService :
        IGenericService<Coupon, CouponDto, CouponCreateDto, CouponUpdateDto>
    {
        Task<CouponDto?> GetCouponByCodeAsync(string code);
        Task<IEnumerable<CouponDto>> GetActiveCouponsAsync();
        Task<CouponValidationResultDto> ValidateCouponAsync(string code, decimal orderAmount);
        Task<decimal> CalculateCouponAmountAsync(string code, decimal orderAmount);
        Task<bool> ApplyCouponAsync(string code);
    }
}
