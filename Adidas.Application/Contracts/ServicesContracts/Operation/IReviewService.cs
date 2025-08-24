using Adidas.DTOs.Common_DTOs;
using Adidas.DTOs.CommonDTOs;
using Adidas.DTOs.Operation.ReviewDTOs;
using Adidas.DTOs.Operation.ReviewDTOs.Query;
using Adidas.DTOs.Operation.ReviewDTOs.Result;

namespace Adidas.Application.Contracts.ServicesContracts.Operation
{


    public interface IReviewService : IGenericService<Review, ReviewDto, ReviewCreateDto, ReviewUpdateDto>
    {
        Task<PagedResultDto<ReviewDto>> GetReviewsByProductIdAsync(Guid productId, int pageNumber, int pageSize);
        Task<IEnumerable<ReviewDto>> GetReviewsByUserIdAsync(string userId);
        Task<ReviewDto> CreateReviewAsync(ReviewCreateDto createReviewDto, string userId);
        // in IReviewService
        Task<PagedResultDto<ReviewDto>> GetFilteredReviewsAsync(ReviewFilterDto filter, int pageNumber, int pageSize);

        Task<(bool Success, string Message)> ApproveReviewAsync(Guid reviewId);
        Task<(bool Success, string Message)> RejectReviewAsync(Guid reviewId, string reason);
        Task<ProductReviewSummaryDto> GetProductReviewSummaryAsync(Guid productId);
        Task<bool> CanUserReviewProductAsync(string userId, Guid productId);
        Task<ReviewStatsDto> GetReviewStatsAsync(); // إضافة method للإحصائيات
    }
}