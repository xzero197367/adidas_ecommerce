
using Adidas.DTOs.Operation.ReviewDTOs;
using Adidas.DTOs.Operation.ReviewDTOs.Query;
using Mapster;

namespace Adidas.Application.Mapping.Operation;

public class ReviewMapConfig
{
    public static void Configure()
    {
        // Get DTO to Model
        TypeAdapterConfig.GlobalSettings.NewConfig<ReviewDto, Review>()
            .IgnoreNullValues(true);

        // Model to Get DTO
        TypeAdapterConfig.GlobalSettings.NewConfig<Review, ReviewDto>();

        // Create DTO to Model
        TypeAdapterConfig.GlobalSettings.NewConfig<ReviewCreateDto, Review>()
            .IgnoreNullValues(true);

        // Update DTO to Model
        TypeAdapterConfig.GlobalSettings.NewConfig<ReviewUpdateDto, Review>()
            .IgnoreNullValues(true);
    }
}