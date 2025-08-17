using Adidas.Application.Contracts.ServicesContracts;
using Adidas.DTOs.Common_DTOs;
using Adidas.DTOs.Feature.CouponDTOs;
using Adidas.Models.Feature;
using Models.Feature;

namespace Adidas.Application.Contracts.ServicesContracts.Feature
{
    public interface ICouponService
    {
        Task<decimal> CalculateCouponAmountAsync(string code, decimal orderAmount);
        Task<CouponApplicationResult>  ApplyCouponToOrderAsync(Guid orderId, string couponCode);
        Task<CouponListResult> GetFilteredPagedCouponsAsync(string search, string status, int page, int pageSize);
        Task<Result> SoftDeletAsync(Guid id);
        Task<Result> ToggleCouponStatusAsync(Guid id);
        Task<Result> UpdateAsync( CouponUpdateDto dto);
        Task<Result> CreateAsync(CouponCreateDto dto);
        Task<CouponUpdateDto> GetCouponToEditByIdAsync(Guid id);
        Task<CouponApplicationResult> ApplyCouponToCartAsync(string userId, string couponCode, decimal cartTotal);

        Task<CouponDetailsDTO> GetCouponDetailsByIdAsync(Guid id);
    }
}
