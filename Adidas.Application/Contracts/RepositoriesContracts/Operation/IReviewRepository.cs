
namespace Adidas.Application.Contracts.RepositoriesContracts.Operation
{
    public interface IReviewRepository : IGenericRepository<Review>
    {
        Task<IEnumerable<Review>> GetReviewsByProductIdAsync(Guid productId);
        Task<IEnumerable<Review>> GetReviewsByUserIdAsync(Guid userId);
        Task<IEnumerable<Review>> GetApprovedReviewsAsync(Guid productId);
        Task<IEnumerable<Review>> GetPendingReviewsAsync();
        Task<double> GetAverageRatingAsync(Guid productId);
        Task<IEnumerable<Review>> GetVerifiedPurchaseReviewsAsync(Guid productId);
        Task<bool> HasUserReviewedProductAsync(Guid userId, Guid productId);
        Task<(IEnumerable<Review> reviews, int totalCount)> GetReviewsPagedAsync(Guid productId, int pageNumber, int pageSize, bool? isApproved = null);
    }
}
 