using Adidas.DTOs.Operation.ReviewDTOs.Create;
using Adidas.DTOs.Operation.ReviewDTOs.Query;
using Adidas.DTOs.Operation.ReviewDTOs.Update;

namespace Adidas.Application.Map.Operation;

public class ReviewMappingProfile: BaseMappingProfile
{
    public ReviewMappingProfile()
    {
        // Review <=> DTOs
        //CreateMap<Review, ReviewDto>();
        //CreateMap<CreateReviewDto, Review>()
        //    .ForMember(dest => dest.IsApproved, opt => opt.MapFrom(src => false))
        //    .ForMember(dest => dest.IsVerifiedPurchase, opt => opt.MapFrom(src => false)); // Needs verification logic
        //CreateMap<UpdateReviewDto, Review>();
        CreateMap<Review, ReviewDto>()
               .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product != null ? src.Product.Name : null))
               .ForMember(dest => dest.UserEmail, opt => opt.MapFrom(src => src.User != null ? src.User.Email : null));

        // CreateReviewDto -> Review
        CreateMap<CreateReviewDto, Review>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => true))
            .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => false))
            .ForMember(dest => dest.IsApproved, opt => opt.MapFrom(src => false)); // Reviews start as unapproved

        // UpdateReviewDto -> Review
        CreateMap<UpdateReviewDto, Review>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
    }

}
