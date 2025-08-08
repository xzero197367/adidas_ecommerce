using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Adidas.DTOs.Common_DTOs;
using Adidas.DTOs.Operation.ReviewDTOs.Create;
using Adidas.DTOs.Operation.ReviewDTOs.Query;
using Adidas.DTOs.Operation.ReviewDTOs.Result;
using Adidas.DTOs.Operation.ReviewDTOs.Update;

namespace Adidas.Application.Contracts.ServicesContracts.Operation
{
    //public interface IReviewService : IGenericService<Review, ReviewDto, CreateReviewDto, UpdateReviewDto>
    //{
    //    Task<PagedResultDto<ReviewDto>> GetReviewsByProductIdAsync(Guid productId, int pageNumber, int pageSize);
    //    Task<IEnumerable<ReviewDto>> GetReviewsByUserIdAsync(string userId);
    //    Task<ReviewDto> CreateReviewAsync(CreateReviewDto createReviewDto);
    //    Task<bool> ApproveReviewAsync(Guid reviewId);
    //    Task<bool> RejectReviewAsync(Guid reviewId, string reason);
    //    Task<ProductReviewSummaryDto> GetProductReviewSummaryAsync(Guid productId);
    //    Task<bool> CanUserReviewProductAsync(string userId, Guid productId);

    //}

    public interface IReviewService : IGenericService<Review, ReviewDto, CreateReviewDto, UpdateReviewDto>
    {
        Task<PagedResultDto<ReviewDto>> GetReviewsByProductIdAsync(Guid productId, int pageNumber, int pageSize);
        Task<IEnumerable<ReviewDto>> GetReviewsByUserIdAsync(string userId);
        Task<ReviewDto> CreateReviewAsync(CreateReviewDto createReviewDto);
        Task<bool> ApproveReviewAsync(Guid reviewId);
        Task<bool> RejectReviewAsync(Guid reviewId, string reason);
        Task<ProductReviewSummaryDto> GetProductReviewSummaryAsync(Guid productId);
        Task<bool> CanUserReviewProductAsync(string userId, Guid productId);
        Task<ReviewStatsDto> GetReviewStatsAsync(); // إضافة method للإحصائيات
    }
}
