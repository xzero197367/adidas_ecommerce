

using Adidas.DTOs.Common_DTOs;
using Adidas.DTOs.Feature.CouponDTOs;

namespace Adidas.Application.Contracts.ServicesContracts.Feature
{
    public interface ICouponService
    {

        Task<OperationResult<CouponDto>> GetCouponByCodeAsync(string code);
        Task<OperationResult<IEnumerable<CouponDto>>> GetActiveCouponsAsync();
        Task<OperationResult<CouponDto>> ValidateCouponAsync(string code, decimal orderAmount);
        Task<decimal> CalculateCouponAmountAsync(string code, decimal orderAmount);
        Task<bool> ApplyCouponAsync(string code);
        
        

    }
}
