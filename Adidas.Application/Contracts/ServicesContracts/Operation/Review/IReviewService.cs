using Adidas.DTOs.Operation.ReviewDTOs.Create;
using Adidas.DTOs.Operation.ReviewDTOs.Query;
using Adidas.DTOs.Operation.ReviewDTOs.Result;
using Adidas.DTOs.Operation.ReviewDTOs.Update;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adidas.Application.Contracts.ServicesContracts.Operation.Review
{
    public interface IReviewService
    {
        Task<ReviewDto?> GetReviewByIdAsync(Guid id);
        Task<ReviewDto> CreateReviewAsync(CreateReviewDto createReviewDto);
        Task<ReviewDto?> UpdateReviewAsync(Guid id, UpdateReviewDto updateReviewDto);
        Task<bool> DeleteReviewAsync(Guid id);

        Task<IEnumerable<ReviewDto>> GetReviewsByProductIdAsync(Guid productId);
        Task<IEnumerable<ReviewDto>> GetReviewsByUserIdAsync(string userId);
        Task<bool> HasUserReviewedProductAsync(string userId, Guid productId);



        Task<PagedReviewDto> GetReviewsPagedAsync(Guid productId, int pageNumber, int pageSize, bool? isApproved = null);
        Task<IEnumerable<ReviewDto>> GetApprovedReviewsAsync(Guid productId);
        Task<IEnumerable<ReviewDto>> GetVerifiedPurchaseReviewsAsync(Guid productId);
        Task<IEnumerable<ReviewDto>> GetReviewsByRatingAsync(Guid productId, int rating);

    }
}
