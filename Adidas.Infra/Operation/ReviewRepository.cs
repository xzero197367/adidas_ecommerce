//using System.Data.Entity;
//namespace Adidas.Infra.Operation
//{
//    public class ReviewRepository : GenericRepository<Review>, IReviewRepository
//    {
//        public ReviewRepository(AdidasDbContext context) : base(context) { }

//        public async Task<IEnumerable<Review>> GetReviewsByProductIdAsync(Guid productId)
//        {
//            return await FindAsync(r => r.ProductId == productId && !r.IsDeleted,
//                                 r => r.User,
//                                 r => r.Product);
//        }

//        public async Task<IEnumerable<Review>> GetReviewsByUserIdAsync(string userId)
//        {
//            return await FindAsync(r => r.UserId == userId && !r.IsDeleted,
//                                 r => r.Product);
//        }

//        public async Task<IEnumerable<Review>> GetApprovedReviewsAsync(Guid productId)
//        {
//            return await FindAsync(r => r.ProductId == productId &&
//                                       r.IsApproved &&
//                                       !r.IsDeleted,
//                                 r => r.User);
//        }

//        public async Task<IEnumerable<Review>> GetPendingReviewsAsync()
//        {
//            return await FindAsync(r => !r.IsApproved && !r.IsDeleted,
//                                 r => r.User,
//                                 r => r.Product);
//        }

//        public async Task<double> GetAverageRatingAsync(Guid productId)  
//        {
//            var query = GetQueryable(r => r.ProductId == productId &&
//                                         r.IsApproved &&
//                                         !r.IsDeleted);

//            var reviews = await query.ToListAsync();
//            return reviews.Any() ? reviews.Average(r => r.Rating) : 0;
//        }

//        public async Task<IEnumerable<Review>> GetVerifiedPurchaseReviewsAsync(Guid productId)
//        {
//            return await FindAsync(r => r.ProductId == productId &&
//                                       r.IsVerifiedPurchase &&
//                                       r.IsApproved &&
//                                       !r.IsDeleted,
//                                 r => r.User);
//        }

//        public async Task<bool> HasUserReviewedProductAsync(string userId, Guid productId)
//        {
//            var count = await CountAsync(r => r.UserId == userId &&
//                                             r.ProductId == productId &&
//                                             !r.IsDeleted);
//            return count > 0;
//        }

//        public async Task<(IEnumerable<Review> reviews, int totalCount)> GetReviewsPagedAsync(Guid productId, int pageNumber, int pageSize, bool? isApproved = null)
//        {
//            if (isApproved.HasValue)
//            {
//                return await GetPagedAsync(pageNumber, pageSize,
//                    r => r.ProductId == productId &&
//                         r.IsApproved == isApproved.Value &&
//                         !r.IsDeleted);
//            }
//            return await GetPagedAsync(pageNumber, pageSize,
//                r => r.ProductId == productId && !r.IsDeleted);
//        }
//    }
//}
using Adidas.Context;

using Adidas.Application.Contracts.RepositoriesContracts.Operation;
using Adidas.DTOs.Common_DTOs;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Adidas.Infra;
using System.Linq.Expressions;

namespace Adidas.Infrastructure.Repositories
{
    //    public class ReviewRepository : GenericRepository<Review>, IReviewRepository
    //    {
    //        private readonly AdidasDbContext _context;

    //        public ReviewRepository(AdidasDbContext context)
    //            : base(context)
    //        {
    //            _context = context ?? throw new ArgumentNullException(nameof(context));
    //        }

    //        public async Task<PagedResultDto<Review>> GetFilteredReviewsAsync(string status, int pageNumber, int pageSize)
    //        {
    //            IQueryable<Review> query = _context.Reviews;

    //            if (!string.IsNullOrEmpty(status))
    //            {
    //                query = status.ToLower() switch
    //                {
    //                    "pending" => query.Where(r => !r.IsApproved && !r.ReviewText.Contains("rejected")),
    //                    "approved" => query.Where(r => r.IsApproved),
    //                    "rejected" => query.Where(r => !r.IsApproved && r.ReviewText.Contains("rejected")),
    //                    _ => query
    //                };
    //            }

    //            var totalCount = await query.CountAsync();

    //            var reviews = await query
    //                .OrderByDescending(r => r.CreatedAt)
    //                .Skip((pageNumber - 1) * pageSize)
    //                .Take(pageSize)
    //                .ToListAsync();

    //            return new PagedResultDto<Review>
    //            {
    //                Items = reviews,
    //                TotalCount = totalCount,
    //                PageNumber = pageNumber,
    //                PageSize = pageSize,
    //                TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
    //            };
    //        }

    //        public async Task<bool> HasUserReviewedProductAsync(string userId, Guid productId)
    //        {
    //            return await _context.Reviews
    //                .AnyAsync(r => r.UserId == userId && r.ProductId == productId);
    //        }

    //        public async Task<IEnumerable<Review>> GetVerifiedPurchaseReviewsAsync(Guid productId)
    //        {
    //            return await _context.Reviews
    //                .Where(r => r.ProductId == productId && r.IsVerifiedPurchase)
    //                .ToListAsync();
    //        }

    //        public async Task<(IEnumerable<Review> reviews, int totalCount)> GetReviewsPagedAsync(Guid productId, int pageNumber, int pageSize, bool? isApproved = null)
    //        {
    //            IQueryable<Review> query = _context.Reviews.Where(r => r.ProductId == productId);

    //            if (isApproved.HasValue)
    //            {
    //                query = query.Where(r => r.IsApproved == isApproved.Value);
    //            }

    //            var totalCount = await query.CountAsync();

    //            var reviews = await query
    //                .OrderByDescending(r => r.CreatedAt)
    //                .Skip((pageNumber - 1) * pageSize)
    //                .Take(pageSize)
    //                .ToListAsync();

    //            return (reviews, totalCount);
    //        }

    //        public async Task<IEnumerable<Review>> GetReviewsByUserIdAsync(string userId)
    //        {
    //            return await _context.Reviews
    //                .Where(r => r.UserId == userId)
    //                .ToListAsync();
    //        }

    //        public async Task<IEnumerable<Review>> GetReviewsByProductIdAsync(Guid productId)
    //        {
    //            return await _context.Reviews
    //                .Where(r => r.ProductId == productId)
    //                .ToListAsync();
    //        }

    //        public async Task<IEnumerable<Review>> GetPendingReviewsAsync()
    //        {
    //            return await _context.Reviews
    //                .Where(r => !r.IsApproved && !r.ReviewText.Contains("rejected"))
    //                .ToListAsync();
    //        }

    //        public async Task<double> GetAverageRatingAsync(Guid productId)
    //        {
    //            var average = await _context.Reviews
    //                .Where(r => r.ProductId == productId)
    //                .AverageAsync(r => (double?)r.Rating) ?? 0.0;

    //            return average;
    //        }

    //        public async Task<IEnumerable<Review>> GetApprovedReviewsAsync(Guid productId)
    //        {
    //            return await _context.Reviews
    //                .Where(r => r.ProductId == productId && r.IsApproved)
    //                .ToListAsync();
    //        }
    //    }
    public class ReviewRepository : GenericRepository<Review>, IReviewRepository
    {
        private readonly AdidasDbContext _context;

        public ReviewRepository(AdidasDbContext context)
            : base(context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        // Override GetPagedAsync to include navigation properties
        public  async Task<(IEnumerable<Review> items, int totalCount)> GetPagedAsync(
            int pageNumber,
            int pageSize,
            Expression<Func<Review, bool>>? predicate = null)
        {
            IQueryable<Review> query = _context.Reviews
                .Include(r => r.User)
                .Include(r => r.Product)
                .Where(r => !r.IsDeleted); // Only get non-deleted reviews

            if (predicate != null)
            {
                query = query.Where(predicate);
            }

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderByDescending(r => r.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }

        public async Task<PagedResultDto<Review>> GetFilteredReviewsAsync(string status, int pageNumber, int pageSize)
        {
            IQueryable<Review> query = _context.Reviews
                .Include(r => r.User)
                .Include(r => r.Product)
                .Where(r => !r.IsDeleted);

            if (!string.IsNullOrEmpty(status))
            {
                query = status.ToLower() switch
                {
                    "pending" => query.Where(r => !r.IsApproved && r.IsActive),
                    "approved" => query.Where(r => r.IsApproved && r.IsActive),
                    "rejected" => query.Where(r => !r.IsApproved && !r.IsActive),
                    _ => query
                };
            }

            var totalCount = await query.CountAsync();

            var reviews = await query
                .OrderByDescending(r => r.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResultDto<Review>
            {
                Items = reviews,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
            };
        }

        public async Task<bool> HasUserReviewedProductAsync(string userId, Guid productId)
        {
            return await _context.Reviews
                .AnyAsync(r => r.UserId == userId && r.ProductId == productId && !r.IsDeleted);
        }

        public async Task<IEnumerable<Review>> GetVerifiedPurchaseReviewsAsync(Guid productId)
        {
            return await _context.Reviews
                .Include(r => r.User)
                .Include(r => r.Product)
                .Where(r => r.ProductId == productId && r.IsVerifiedPurchase && !r.IsDeleted)
                .ToListAsync();
        }

        public async Task<(IEnumerable<Review> reviews, int totalCount)> GetReviewsPagedAsync(
            Guid productId,
            int pageNumber,
            int pageSize,
            bool? isApproved = null)
        {
            IQueryable<Review> query = _context.Reviews
                .Include(r => r.User)
                .Include(r => r.Product)
                .Where(r => r.ProductId == productId && !r.IsDeleted);

            if (isApproved.HasValue)
            {
                query = query.Where(r => r.IsApproved == isApproved.Value);
            }

            var totalCount = await query.CountAsync();

            var reviews = await query
                .OrderByDescending(r => r.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (reviews, totalCount);
        }

        public async Task<IEnumerable<Review>> GetReviewsByUserIdAsync(string userId)
        {
            return await _context.Reviews
                .Include(r => r.User)
                .Include(r => r.Product)
                .Where(r => r.UserId == userId && !r.IsDeleted)
                .ToListAsync();
        }

        public async Task<IEnumerable<Review>> GetReviewsByProductIdAsync(Guid productId)
        {
            return await _context.Reviews
                .Include(r => r.User)
                .Include(r => r.Product)
                .Where(r => r.ProductId == productId && !r.IsDeleted)
                .ToListAsync();
        }

        public async Task<IEnumerable<Review>> GetPendingReviewsAsync()
        {
            return await _context.Reviews
                .Include(r => r.User)
                .Include(r => r.Product)
                .Where(r => !r.IsApproved && r.IsActive && !r.IsDeleted)
                .ToListAsync();
        }

        public async Task<double> GetAverageRatingAsync(Guid productId)
        {
            var average = await _context.Reviews
                .Where(r => r.ProductId == productId && r.IsApproved && !r.IsDeleted)
                .AverageAsync(r => (double?)r.Rating) ?? 0.0;

            return average;
        }

        public async Task<IEnumerable<Review>> GetApprovedReviewsAsync(Guid productId)
        {
            return await _context.Reviews
                .Include(r => r.User)
                .Include(r => r.Product)
                .Where(r => r.ProductId == productId && r.IsApproved && !r.IsDeleted)
                .ToListAsync();
        }

        // إضافة methods جديدة لإحصائيات أفضل
        public async Task<int> GetRejectedReviewsCountAsync()
        {
            return await _context.Reviews
                .CountAsync(r => !r.IsApproved && !r.IsActive && !r.IsDeleted);
        }

        public async Task<int> GetPendingReviewsCountAsync()
        {
            return await _context.Reviews
                .CountAsync(r => !r.IsApproved && r.IsActive && !r.IsDeleted);
        }
    }
}
