using Adidas.DTOs.Feature.CouponDTOs;
using Adidas.DTOs.Feature.OrderCouponDTOs;
using Adidas.Models.Feature;

namespace Adidas.Application.Map.Feature;

public class CouponOrderMappingProfile: BaseMappingProfile
{

    public CouponOrderMappingProfile()
    {
        // OrderCoupon mappings
        CreateMap<OrderCoupon, OrderCouponDto>();
        CreateMap<OrderCouponCreateDto, OrderCoupon>();
        CreateMap<OrderCouponUpdateDto, OrderCoupon>();
        CreateMap<OrderCoupon, CouponUpdateDto>();
    }
}