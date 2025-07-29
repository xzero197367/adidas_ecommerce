

using Adidas.DTOs.Feature.CouponDTOs;

namespace Adidas.Application.Contracts.ServicesContracts.Feature
{
    public interface ICouponService
    {

        Task<CouponDto?> GetCouponByCodeAsync(string code);
        Task<IEnumerable<CouponDto>> GetActiveCouponsAsync();
        Task<CouponValidationResultDto> ValidateCouponAsync(string code, decimal orderAmount);
        Task<decimal> CalculateCouponAmountAsync(string code, decimal orderAmount);
        Task<bool> ApplyCouponAsync(string code);

    }
}
