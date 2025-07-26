using System.Data.Entity;
namespace Adidas.Infra.Operation
{
    public class ReviewRepository : GenericRepository<Review>, IReviewRepository
    {
        public ReviewRepository(AdidasDbContext context) : base(context) { }

        public async Task<IEnumerable<Review>> GetReviewsByProductIdAsync(Guid productId)
        {
            return await FindAsync(r => r.ProductId == productId && !r.IsDeleted,
                                 r => r.User,
                                 r => r.Product);
        }

        public async Task<IEnumerable<Review>> GetReviewsByUserIdAsync(string userId)
        {
            return await FindAsync(r => r.UserId == userId && !r.IsDeleted,
                                 r => r.Product);
        }

        public async Task<IEnumerable<Review>> GetApprovedReviewsAsync(Guid productId)
        {
            return await FindAsync(r => r.ProductId == productId &&
                                       r.IsApproved &&
                                       !r.IsDeleted,
                                 r => r.User);
        }

        public async Task<IEnumerable<Review>> GetPendingReviewsAsync()
        {
            return await FindAsync(r => !r.IsApproved && !r.IsDeleted,
                                 r => r.User,
                                 r => r.Product);
        }

        public async Task<double> GetAverageRatingAsync(Guid productId)  
        {
            var query = GetQueryable(r => r.ProductId == productId &&
                                         r.IsApproved &&
                                         !r.IsDeleted);

            var reviews = await query.ToListAsync();
            return reviews.Any() ? reviews.Average(r => r.Rating) : 0;
        }

        public async Task<IEnumerable<Review>> GetVerifiedPurchaseReviewsAsync(Guid productId)
        {
            return await FindAsync(r => r.ProductId == productId &&
                                       r.IsVerifiedPurchase &&
                                       r.IsApproved &&
                                       !r.IsDeleted,
                                 r => r.User);
        }

        public async Task<bool> HasUserReviewedProductAsync(string userId, Guid productId)
        {
            var count = await CountAsync(r => r.UserId == userId &&
                                             r.ProductId == productId &&
                                             !r.IsDeleted);
            return count > 0;
        }

        public async Task<(IEnumerable<Review> reviews, int totalCount)> GetReviewsPagedAsync(Guid productId, int pageNumber, int pageSize, bool? isApproved = null)
        {
            if (isApproved.HasValue)
            {
                return await GetPagedAsync(pageNumber, pageSize,
                    r => r.ProductId == productId &&
                         r.IsApproved == isApproved.Value &&
                         !r.IsDeleted);
            }
            return await GetPagedAsync(pageNumber, pageSize,
                r => r.ProductId == productId && !r.IsDeleted);
        }
    }
}
