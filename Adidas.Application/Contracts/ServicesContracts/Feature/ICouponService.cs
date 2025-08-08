using Adidas.Application.Contracts.ServicesContracts;
using Adidas.DTOs.Common_DTOs;
using Adidas.DTOs.Feature.CouponDTOs;
using Adidas.Models.Feature;
using Models.Feature;

namespace Adidas.Application.Contracts.ServicesContracts.Feature
{
    public interface ICouponService //:IGenericService<Coupon, CouponDto, CouponCreateDto, CouponUpdateDto>
    {
        Task<List<CouponDto>> GetAllCouponsAsync();
        Task<IEnumerable<CouponDto>> GetActiveCouponsAsync();
        Task<CouponValidationResultDto> ValidateCouponAsync(string code, decimal orderAmount);
        Task<decimal> CalculateCouponAmountAsync(string code, decimal orderAmount);
        Task<bool> ApplyCouponAsync(string code);
        Task<IEnumerable<CouponDto>> GetFilteredCouponsAsync(string search,string status);
        Task<CouponListResult> GetFilteredPagedCouponsAsync(string search, string status, int page, int pageSize);
        Task<Result> SoftDeletAsync(Guid id);
        Task<Result> ToggleCouponStatusAsync(Guid id);
        Task<CouponDto> GetByIdAsync(Guid id);
        Task<Result> UpdateAsync( CouponUpdateDto dto);
        Task<Result> CreateAsync(CouponCreateDto dto);
        Task<CouponUpdateDto> GetCouponToEditByIdAsync(Guid id);
    }
}
