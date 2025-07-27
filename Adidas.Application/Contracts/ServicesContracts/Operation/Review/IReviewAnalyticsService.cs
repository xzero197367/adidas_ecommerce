using Adidas.DTOs.Operation.ReviewDTOs.Result;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adidas.Application.Contracts.ServicesContracts.Operation.Review
{
    public interface IReviewAnalyticsService
    {
        Task<double> GetAverageRatingAsync(Guid productId);
        Task<ReviewStatsDto> GetReviewStatsAsync(Guid? productId = null);
        Task<ProductReviewSummaryDto> GetProductReviewSummaryAsync(Guid productId);
        Task<UserReviewSummaryDto> GetUserReviewSummaryAsync(string userId);
        Task<Dictionary<int, int>> GetRatingDistributionAsync(Guid productId);
        Task<ReviewStatsDto> GetUserReviewingStatsAsync(string userId);

    }
}
