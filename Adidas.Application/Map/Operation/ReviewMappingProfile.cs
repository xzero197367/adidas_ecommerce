using Adidas.DTOs.Operation.ReviewDTOs.Create;
using Adidas.DTOs.Operation.ReviewDTOs.Query;
using Adidas.DTOs.Operation.ReviewDTOs.Update;

namespace Adidas.Application.Map.Operation;

public class ReviewMappingProfile: BaseMappingProfile
{
    public ReviewMappingProfile()
    {
        // Review <=> DTOs
        CreateMap<Review, ReviewDto>();
        CreateMap<CreateReviewDto, Review>()
            .ForMember(dest => dest.IsApproved, opt => opt.MapFrom(src => false))
            .ForMember(dest => dest.IsVerifiedPurchase, opt => opt.MapFrom(src => false)); // Needs verification logic
        CreateMap<UpdateReviewDto, Review>();

    }
}