
//using Adidas.DTOs.Common_DTOs;
//using Adidas.DTOs.Operation.ReviewDTOs.Query;
//using Adidas.DTOs.Operation.ReviewDTOs.Result;

//namespace Adidas.Application.Contracts.RepositoriesContracts.Operation
//{
//    public interface IReviewRepository : IGenericRepository<Review>
//    {

//        Task<IEnumerable<Review>> GetReviewsByProductIdAsync(Guid productId);
//        Task<IEnumerable<Review>> GetReviewsByUserIdAsync(string userId);
//        Task<IEnumerable<Review>> GetApprovedReviewsAsync(Guid productId);
//        Task<IEnumerable<Review>> GetPendingReviewsAsync();
//        Task<double> GetAverageRatingAsync(Guid productId);
//        Task<IEnumerable<Review>> GetVerifiedPurchaseReviewsAsync(Guid productId);
//        Task<bool> HasUserReviewedProductAsync(string userId, Guid productId);
//        Task<PagedResultDto<Review>> GetFilteredReviewsAsync(string status, int pageNumber, int pageSize);
//        Task<(IEnumerable<Review> reviews, int totalCount)> GetReviewsPagedAsync(Guid productId, int pageNumber, int pageSize, bool? isApproved = null);

//        // إضافة methods جديدة للإحصائيات
//        Task<int> GetRejectedReviewsCountAsync();
//        Task<int> GetPendingReviewsCountAsync();
//    }

//}
using Adidas.DTOs.Common_DTOs;
using Adidas.DTOs.Operation.ReviewDTOs.Query;
using Adidas.DTOs.Operation.ReviewDTOs.Result;

namespace Adidas.Application.Contracts.RepositoriesContracts.Operation
{
    public interface IReviewRepository : IGenericRepository<Review>
    {
        Task<IEnumerable<Review>> GetReviewsByProductIdAsync(Guid productId);
        Task<IEnumerable<Review>> GetReviewsByUserIdAsync(string userId);
        Task<IEnumerable<Review>> GetApprovedReviewsAsync(Guid productId);
        Task<IEnumerable<Review>> GetPendingReviewsAsync();
        Task<double> GetAverageRatingAsync(Guid productId);
        Task<IEnumerable<Review>> GetVerifiedPurchaseReviewsAsync(Guid productId);
        Task<bool> HasUserReviewedProductAsync(string userId, Guid productId);
        Task<PagedResultDto<Review>> GetFilteredReviewsAsync(string status, int pageNumber, int pageSize);
        Task<(IEnumerable<Review> reviews, int totalCount)> GetReviewsPagedAsync(Guid productId, int pageNumber, int pageSize, bool? isApproved = null);

        // ✅ UPDATED: Statistics methods
        Task<int> GetRejectedReviewsCountAsync();
        Task<int> GetPendingReviewsCountAsync();
        Task<int> GetApprovedReviewsCountAsync(); // ✅ NEW: Added for completeness
    }
}