using Adidas.Application.Contracts.ServicesContracts;
using Adidas.DTOs.Common_DTOs;
using Adidas.DTOs.Feature.CouponDTOs;
using Adidas.Models.Feature;
using Models.Feature;

namespace Adidas.Application.Contracts.ServicesContracts.Feature
{
    public interface ICouponService :
        IGenericService<Coupon, CouponDto, CouponCreateDto, CouponUpdateDto>
    {
        Task<List<CouponDto>> GetAllCouponsAsync();
        Task<OperationResult<IEnumerable<CouponDto>>> GetActiveCouponsAsync();
        Task<OperationResult<CouponDto>> ValidateCouponAsync(string code, decimal orderAmount);
        Task<decimal> CalculateCouponAmountAsync(string code, decimal orderAmount);
        Task<bool> ApplyCouponAsync(string code);
    }
}
