
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
    public class ReviewRepository : GenericRepository<Review>, IReviewRepository
    {
        private readonly AdidasDbContext _context;

        public ReviewRepository(AdidasDbContext context)
            : base(context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        // Override GetPagedAsync to include navigation properties
        public async Task<(IEnumerable<Review> items, int totalCount)> GetPagedAsync(
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

        // ✅ FIXED: Updated GetFilteredReviewsAsync method with proper status filtering
        public async Task<PagedResultDto<Review>> GetFilteredReviewsAsync(string status, int pageNumber, int pageSize)
        {
            IQueryable<Review> query = _context.Reviews
                .Include(r => r.User)
                .Include(r => r.Product)
                .Where(r => !r.IsDeleted);

            // ✅ FIXED: Proper status filtering logic
            if (!string.IsNullOrEmpty(status))
            {
                query = status.ToLower() switch
                {
                    // Pending: not approved but active
                    "pending" => query.Where(r => !r.IsApproved && r.IsActive),
                    // Approved: approved (and typically active)
                    "approved" => query.Where(r => r.IsApproved && r.IsActive),
                    // Rejected: not approved and not active
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
        // ✅ NEW: Search reviews by product name with pagination
        public async Task<PagedResultDto<Review>> GetReviewsByProductNameAsync(
    string productName, int pageNumber, int pageSize)
        {
            IQueryable<Review> query = _context.Reviews
                .Include(r => r.User)
                .Include(r => r.Product)
                .Where(r => !r.IsDeleted);

            if (!string.IsNullOrWhiteSpace(productName))
            {
                query = query.Where(r =>
                    r.Product != null &&
                    r.Product.Name.ToLower().Contains(productName.ToLower()));
            }

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderByDescending(r => r.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResultDto<Review>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
            };
        }


        // ✅ FIXED: Statistics methods with proper logic
        public async Task<int> GetRejectedReviewsCountAsync()
        {
            // Rejected = IsApproved = false AND IsActive = false AND not deleted
            return await _context.Reviews
                .CountAsync(r => !r.IsApproved && !r.IsActive && !r.IsDeleted);
        }

        public async Task<int> GetPendingReviewsCountAsync()
        {
            // Pending = IsApproved = false AND IsActive = true AND not deleted
            return await _context.Reviews
                .CountAsync(r => !r.IsApproved && r.IsActive && !r.IsDeleted);
        }

        // ✅ NEW: Added for completeness
        public async Task<int> GetApprovedReviewsCountAsync()
        {
            // Approved = IsApproved = true AND not deleted
            return await _context.Reviews
                .CountAsync(r => r.IsApproved && !r.IsDeleted);
        }
    }
}