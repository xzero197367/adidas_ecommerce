
using Adidas.DTOs.CommonDTOs;
using Adidas.DTOs.Feature.CouponDTOs;
using Models.Feature;

namespace Adidas.Application.Contracts.ServicesContracts.Feature
{
    public interface ICouponService: IGenericService<Coupon, CouponDto, CouponCreateDto, CouponUpdateDto>
    {
        
        Task<OperationResult<IEnumerable<CouponDto>>> GetAllCouponsAsync();
        Task<OperationResult<IEnumerable<CouponDto>>> GetActiveCouponsAsync();
        Task<OperationResult<CouponDto>> ValidateCouponAsync(string code, decimal orderAmount);
        Task<decimal> CalculateCouponAmountAsync(string code, decimal orderAmount);
        Task<bool> ApplyCouponAsync(string code);
    }
}