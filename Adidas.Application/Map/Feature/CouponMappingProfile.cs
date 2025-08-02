using Adidas.DTOs.Feature.CouponDTOs;
using Adidas.Models.Feature;
using AutoMapper;
using Adidas.Application.Map;
using Models.Feature;

namespace Adidas.Application.Map.Feature
{
    public class CouponMappingProfile : BaseMappingProfile
    {
        public CouponMappingProfile()
        {
            // Coupon mappings
            CreateMap<Coupon, CouponDto>();
            CreateMap<CouponCreateDto, Coupon>();
            CreateMap<CouponUpdateDto, Coupon>();
            CreateMap<Coupon, CouponUpdateDto>();
        }
        
        public static void RegisterMappings(IMapperConfigurationExpression cfg)
        {
            cfg.CreateMap<Coupon, CouponDto>();
            cfg.CreateMap<CouponCreateDto, Coupon>();
            cfg.CreateMap<CouponUpdateDto, Coupon>();
            cfg.CreateMap<Coupon, CouponUpdateDto>();
        }
    }
}
