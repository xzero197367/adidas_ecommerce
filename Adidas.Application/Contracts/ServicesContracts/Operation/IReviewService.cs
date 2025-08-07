
using Adidas.DTOs.Common_DTOs;
using Adidas.DTOs.CommonDTOs;
using Adidas.DTOs.Operation.ReviewDTOs;
using Adidas.DTOs.Operation.ReviewDTOs.Query;
using Adidas.DTOs.Operation.ReviewDTOs.Result;

namespace Adidas.Application.Contracts.ServicesContracts.Operation
{
    public interface IReviewService : IGenericService<Review, ReviewDto, ReviewCreateDto, ReviewUpdateDto>
    {
        Task<OperationResult<PagedResultDto<ReviewDto>>> GetReviewsByProductIdAsync(Guid productId, int pageNumber, int pageSize);
        Task<OperationResult<IEnumerable<ReviewDto>>> GetReviewsByUserIdAsync(string userId);
        Task<OperationResult<ReviewDto>> CreateReviewAsync(ReviewCreateDto createReviewDto);
        Task<OperationResult<bool>> ApproveReviewAsync(Guid reviewId);
        Task<OperationResult<bool>> RejectReviewAsync(Guid reviewId, string reason);
        Task<OperationResult<ProductReviewSummaryDto>> GetProductReviewSummaryAsync(Guid productId);
        Task<OperationResult<bool>> CanUserReviewProductAsync(string userId, Guid productId);
    }
}
