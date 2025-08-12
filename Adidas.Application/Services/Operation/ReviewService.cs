
//using Microsoft.Extensions.Logging;
//using Adidas.Application.Contracts.RepositoriesContracts.Operation;
//using Adidas.Application.Contracts.ServicesContracts.Operation;
//using Adidas.DTOs.Operation.ReviewDTOs.Query;
//using Adidas.DTOs.Operation.ReviewDTOs.Result;
//using Adidas.DTOs.Common_DTOs;
//using Adidas.DTOs.Operation.ReviewDTOs;
//using Mapster;
//using Microsoft.EntityFrameworkCore;
//using Microsoft.AspNetCore.Http;
//using Adidas.DTOs.CommonDTOs;


//namespace Adidas.Application.Services.Operation
//{
//    public class ReviewService : GenericService<Review, ReviewDto, ReviewCreateDto, ReviewUpdateDto>, IReviewService
//    {
//        private readonly IReviewRepository _reviewRepository;
//        private readonly IOrderRepository _orderRepository;

//        public ReviewService(
//            IReviewRepository reviewRepository,
//            IOrderRepository orderRepository,
//            ILogger<ReviewService> logger)
//            : base(reviewRepository, logger)
//        {
//            _reviewRepository = reviewRepository;
//            _orderRepository = orderRepository;
//        }

//        public async Task<PagedResultDto<ReviewDto>> GetReviewsByProductIdAsync(Guid productId, int pageNumber, int pageSize)
//        {
//            var (reviews, totalCount) = await _reviewRepository.GetReviewsPagedAsync(productId, pageNumber, pageSize);

//            return new PagedResultDto<ReviewDto>
//            {
//                Items = reviews.Adapt<IEnumerable<ReviewDto>>(),
//                TotalCount = totalCount,
//                PageNumber = pageNumber,
//                PageSize = pageSize,
//                TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
//            };
//        }

//        public async Task<IEnumerable<ReviewDto>> GetReviewsByUserIdAsync(string userId)
//        {
//            var reviews = await _reviewRepository.GetReviewsByUserIdAsync(userId);
//            return reviews.Adapt<IEnumerable<ReviewDto>>();
//        }

//        public async Task<ReviewDto> CreateReviewAsync(ReviewCreateDto createReviewDto)
//        {
//            if (!await CanUserReviewProductAsync(createReviewDto.UserId, createReviewDto.ProductId))
//                throw new InvalidOperationException("User cannot review this product");

//            await ValidateCreateAsync(createReviewDto);

//            var review = createReviewDto.Adapt<Review>();
//            review.IsApproved = false; // Reviews need approval
//            review.IsActive = true;
//            review.IsDeleted = false;

//            await BeforeCreateAsync(review);
//            var createdReviewEntry = await _reviewRepository.AddAsync(review);
//            await _reviewRepository.SaveChangesAsync();

//            var createdReview = createdReviewEntry.Entity;
//            await AfterCreateAsync(createdReview);

//            return createdReview.Adapt<ReviewDto>();
//        }

//        public async Task<bool> ApproveReviewAsync(Guid reviewId)
//        {
//            try
//            {
//                var review = await _reviewRepository.GetByIdAsync(reviewId);
//                if (review == null) return false;

//                review.IsApproved = true;
//                review.IsActive = true;
//                review.UpdatedAt = DateTime.UtcNow;

//                await _reviewRepository.UpdateAsync(review);
//                await _reviewRepository.SaveChangesAsync();
//                return true;
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Error approving review {ReviewId}", reviewId);
//                return false;
//            }
//        }

//        public async Task<bool> RejectReviewAsync(Guid reviewId, string reason)
//        {
//            try
//            {
//                var review = await _reviewRepository.GetByIdAsync(reviewId);
//                if (review == null) return false;

//                review.IsApproved = false;
//                review.IsActive = false; // Mark as inactive to indicate rejection
//                review.UpdatedAt = DateTime.UtcNow;
//                // يمكنك إضافة حقل RejectReason إلى الـ model إذا كنت تريد حفظ السبب

//                await _reviewRepository.UpdateAsync(review);
//                await _reviewRepository.SaveChangesAsync();
//                return true;
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Error rejecting review {ReviewId}", reviewId);
//                return false;
//            }
//        }

//        public async Task<ProductReviewSummaryDto> GetProductReviewSummaryAsync(Guid productId)
//        {
//            var reviews = await _reviewRepository.GetApprovedReviewsAsync(productId);
//            var averageRating = await _reviewRepository.GetAverageRatingAsync(productId);

//            var ratingDistribution = reviews
//                .GroupBy(r => r.Rating)
//                .ToDictionary(g => g.Key, g => g.Count());

//            return new ProductReviewSummaryDto
//            {
//                ProductId = productId,
//                AverageRating = averageRating,
//                TotalReviews = reviews.Count(),
//                RatingDistribution = ratingDistribution
//            };
//        }


//public async Task<PagedResultDto<ReviewDto>> GetFilteredReviewsAsync(ReviewFilterDto filter, int pageNumber, int pageSize)
//    {
//        // Start with base query (non-deleted) and include navigation props for UI mapping
//        IQueryable<Review> query = _reviewRepository.GetAll(q =>
//            q.Include(r => r.User)
//             .Include(r => r.Product)
//        ).Where(r => !r.IsDeleted);

//        // Apply status filter (status string or explicit booleans)
//        if (filter != null)
//        {
//            // Example: status could be represented using IsApproved and IsActive flags in DB
//            // Support explicit IsApproved or status string mapping if your UI sends it
//            if (filter.IsApproved.HasValue)
//            {
//                query = query.Where(r => r.IsApproved == filter.IsApproved.Value);
//            }

//            if (filter.MinRating.HasValue)
//            {
//                query = query.Where(r => r.Rating >= filter.MinRating.Value);
//            }

//            if (filter.MaxRating.HasValue)
//            {
//                query = query.Where(r => r.Rating <= filter.MaxRating.Value);
//            }

//            if (filter.IsVerifiedPurchase.HasValue)
//            {
//                query = query.Where(r => r.IsVerifiedPurchase == filter.IsVerifiedPurchase.Value);
//            }

//            if (filter.ProductId.HasValue)
//            {
//                query = query.Where(r => r.ProductId == filter.ProductId.Value);
//            }

//            if (!string.IsNullOrWhiteSpace(filter.UserId))
//            {
//                query = query.Where(r => r.UserId == filter.UserId);
//            }

//            if (filter.StartDate.HasValue)
//            {
//                var s = filter.StartDate.Value.Date;
//                query = query.Where(r => r.CreatedAt >= s);
//            }

//            if (filter.EndDate.HasValue)
//            {
//                var e = filter.EndDate.Value.Date.AddDays(1).AddTicks(-1);
//                query = query.Where(r => r.CreatedAt <= e);
//            }

//            if (!string.IsNullOrWhiteSpace(filter.SearchText))
//            {
//                var term = filter.SearchText.Trim();
//                // Search in title, review text, product name or user email if available
//                query = query.Where(r =>
//                    EF.Functions.Like(r.Title, $"%{term}%") ||
//                    EF.Functions.Like(r.ReviewText, $"%{term}%") ||
//                    (r.Product != null && EF.Functions.Like(r.Product.Name, $"%{term}%")) ||
//                    (r.User != null && EF.Functions.Like(r.User.Email, $"%{term}%"))
//                );
//            }
//        }

//        // Get total count
//        var totalCount = await query.CountAsync();

//        var items = await query
//            .OrderByDescending(r => r.CreatedAt)
//            .Skip((pageNumber - 1) * pageSize)
//            .Take(pageSize)
//            .ToListAsync();

//        // Map to DTO and return PagedResultDto
//        var mapped = items.Adapt<IEnumerable<ReviewDto>>();

//        return new PagedResultDto<ReviewDto>
//        {
//            Items = mapped,
//            TotalCount = totalCount,
//            PageNumber = pageNumber,
//            PageSize = pageSize,
//            TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
//        };
//    }


//        //private async Task<ReviewStatsDto> GetReviewStatsAsync()
//        //{
//        //    // Delegate to the service which already computes Total/Approved/Pending/Rejected.
//        //    return await _reviewService.GetReviewStatsAsync();
//        //}

//        public async Task SoftDeleteAsync(Guid id)
//        {
//            var entity = await _reviewRepository.GetByIdAsync(id);
//            if (entity == null) throw new KeyNotFoundException("Review not found.");

//            // Example: extra logic before delete
//            if (entity.IsApproved )
//            {
//                throw new UnauthorizedAccessException("Only admins can delete approved reviews.");
//            }

//            entity.IsDeleted = true;
//            await _reviewRepository.UpdateAsync(entity);
//        }

//        //Optional helper
//        //private bool IsCurrentUserAdmin()
//        //{
//        //    return _httpContextAccessor.HttpContext.User.IsInRole("Admin");
//        //}


//        public async Task<bool> CanUserReviewProductAsync(string userId, Guid productId)
//        {
//            try
//            {
//                // Check if user has already reviewed this product
//                if (await _reviewRepository.HasUserReviewedProductAsync(userId, productId))
//                    return false;

//                // Check if user has purchased this product
//                var userOrders = await _orderRepository.GetOrdersByUserIdAsync(userId);
//                var hasOrderedProduct = userOrders
//                    .Where(o => o.OrderStatus == OrderStatus.Delivered)
//                    .SelectMany(o => o.OrderItems)
//                    .Any(oi => oi.Variant.ProductId == productId);

//                return hasOrderedProduct;
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Error checking if user {UserId} can review product {ProductId}", userId, productId);
//                return false;
//            }
//        }


//        public async Task<ReviewStatsDto> GetReviewStatsAsync()
//        {
//            try
//            {
//                var totalReviews = await _repository.CountAsync(r => !r.IsDeleted);
//                var approvedReviews = await _repository.CountAsync(r => r.IsApproved && !r.IsDeleted);
//                var pendingReviews = await _reviewRepository.GetPendingReviewsCountAsync();
//                var rejectedReviews = await _reviewRepository.GetRejectedReviewsCountAsync();
//                var verifiedPurchases = await _repository.CountAsync(r => r.IsVerifiedPurchase && !r.IsDeleted);

//                var allApprovedReviews = await _repository.GetAll()
//                    .Where(r => r.IsApproved && !r.IsDeleted)
//                    .ToListAsync();
//                var averageRating = allApprovedReviews.Any()
//                    ? allApprovedReviews.Average(r => r.Rating)
//                    : 0.0;

//                return new ReviewStatsDto
//                {
//                    TotalReviews = totalReviews,
//                    ApprovedReviews = approvedReviews,
//                    PendingReviews = pendingReviews,
//                    RejectedReviews = rejectedReviews,
//                    AverageRating = Math.Round(averageRating, 2),
//                    VerifiedPurchases = verifiedPurchases
//                };
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Error getting review statistics");
//                return new ReviewStatsDto();
//            }
//        }


//        public override async Task ValidateCreateAsync(ReviewCreateDto createDto)
//        {
//            if (createDto.Rating < 1 || createDto.Rating > 5)
//                throw new ArgumentException("Rating must be between 1 and 5");

//            if (string.IsNullOrWhiteSpace(createDto.Title))
//                throw new ArgumentException("Review title is required");

//            if (string.IsNullOrWhiteSpace(createDto.ReviewText))
//                throw new ArgumentException("Review comment is required");

//            // Additional validation
//            if (createDto.Title.Length > 200)
//                throw new ArgumentException("Review title cannot exceed 200 characters");

//            if (!string.IsNullOrEmpty(createDto.ReviewText) && createDto.ReviewText.Length > 1000)
//                throw new ArgumentException("Review text cannot exceed 1000 characters");

//            await Task.CompletedTask;
//        }

//        public override async Task ValidateUpdateAsync(Guid id, ReviewUpdateDto updateDto)
//        {
//            var existingReview = await _repository.GetByIdAsync(id);
//            if (existingReview == null)
//                throw new KeyNotFoundException($"Review with ID {id} not found");

//            if (updateDto.Rating.HasValue && (updateDto.Rating < 1 || updateDto.Rating > 5))
//                throw new ArgumentException("Rating must be between 1 and 5");

//            if (!string.IsNullOrEmpty(updateDto.Title) && updateDto.Title.Length > 200)
//                throw new ArgumentException("Review title cannot exceed 200 characters");

//            if (!string.IsNullOrEmpty(updateDto.ReviewText) && updateDto.ReviewText.Length > 1000)
//                throw new ArgumentException("Review text cannot exceed 1000 characters");

//            await Task.CompletedTask;
//        }

//        public override async Task BeforeCreateAsync(Review entity)
//        {
//            entity.CreatedAt = DateTime.UtcNow;
//            entity.UpdatedAt = DateTime.UtcNow;
//            await Task.CompletedTask;
//        }

//        public override async Task BeforeUpdateAsync(Review entity)
//        {
//            entity.UpdatedAt = DateTime.UtcNow;
//            await Task.CompletedTask;
//        }

//        public override async Task AfterCreateAsync(Review entity)
//        {
//            _logger.LogInformation("New review created: ID {ReviewId} for Product {ProductId} by User {UserId}",
//                entity.Id, entity.ProductId, entity.UserId);
//            await Task.CompletedTask;
//        }

//        public override async Task AfterUpdateAsync(Review entity)
//        {
//            _logger.LogInformation("Review updated: ID {ReviewId}", entity.Id);
//            await Task.CompletedTask;
//        }
//    }
//}
using Microsoft.Extensions.Logging;
using Adidas.Application.Contracts.RepositoriesContracts.Operation;
using Adidas.Application.Contracts.ServicesContracts.Operation;
using Adidas.DTOs.Operation.ReviewDTOs.Query;
using Adidas.DTOs.Operation.ReviewDTOs.Result;
using Adidas.DTOs.Common_DTOs;
using Adidas.DTOs.Operation.ReviewDTOs;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Adidas.DTOs.CommonDTOs;

namespace Adidas.Application.Services.Operation
{
    public class ReviewService : GenericService<Review, ReviewDto, ReviewCreateDto, ReviewUpdateDto>, IReviewService
    {
        private readonly IReviewRepository _reviewRepository;
        private readonly IOrderRepository _orderRepository;

        public ReviewService(
            IReviewRepository reviewRepository,
            IOrderRepository orderRepository,
            ILogger<ReviewService> logger)
            : base(reviewRepository, logger)
        {
            _reviewRepository = reviewRepository;
            _orderRepository = orderRepository;
        }

        public async Task<PagedResultDto<ReviewDto>> GetReviewsByProductIdAsync(Guid productId, int pageNumber, int pageSize)
        {
            var (reviews, totalCount) = await _reviewRepository.GetReviewsPagedAsync(productId, pageNumber, pageSize);

            return new PagedResultDto<ReviewDto>
            {
                Items = reviews.Adapt<IEnumerable<ReviewDto>>(),
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
            };
        }

        public async Task<IEnumerable<ReviewDto>> GetReviewsByUserIdAsync(string userId)
        {
            var reviews = await _reviewRepository.GetReviewsByUserIdAsync(userId);
            return reviews.Adapt<IEnumerable<ReviewDto>>();
        }

        public async Task<ReviewDto> CreateReviewAsync(ReviewCreateDto createReviewDto)
        {
            if (!await CanUserReviewProductAsync(createReviewDto.UserId, createReviewDto.ProductId))
                throw new InvalidOperationException("User cannot review this product");

            await ValidateCreateAsync(createReviewDto);

            var review = createReviewDto.Adapt<Review>();
            review.IsApproved = false; // Reviews need approval
            review.IsActive = true;
            review.IsDeleted = false;

            await BeforeCreateAsync(review);
            var createdReviewEntry = await _reviewRepository.AddAsync(review);
            await _reviewRepository.SaveChangesAsync();

            var createdReview = createdReviewEntry.Entity;
            await AfterCreateAsync(createdReview);

            return createdReview.Adapt<ReviewDto>();
        }

        public async Task<bool> ApproveReviewAsync(Guid reviewId)
        {
            try
            {
                var review = await _reviewRepository.GetByIdAsync(reviewId);
                if (review == null) return false;

                review.IsApproved = true;
                review.IsActive = true;
                review.UpdatedAt = DateTime.UtcNow;

                await _reviewRepository.UpdateAsync(review);
                await _reviewRepository.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error approving review {ReviewId}", reviewId);
                return false;
            }
        }

        // ✅ FIXED: Proper rejection logic
        public async Task<bool> RejectReviewAsync(Guid reviewId, string reason)
        {
            try
            {
                var review = await _reviewRepository.GetByIdAsync(reviewId);
                if (review == null) return false;

                // ✅ FIXED: Set IsApproved = false AND IsActive = false for rejection
                review.IsApproved = false;
                review.IsActive = false; // This marks it as rejected
                review.UpdatedAt = DateTime.UtcNow;
                // You can add a RejectReason field to the model if needed

                await _reviewRepository.UpdateAsync(review);
                await _reviewRepository.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rejecting review {ReviewId}", reviewId);
                return false;
            }
        }

        public async Task<ProductReviewSummaryDto> GetProductReviewSummaryAsync(Guid productId)
        {
            var reviews = await _reviewRepository.GetApprovedReviewsAsync(productId);
            var averageRating = await _reviewRepository.GetAverageRatingAsync(productId);

            var ratingDistribution = reviews
                .GroupBy(r => r.Rating)
                .ToDictionary(g => g.Key, g => g.Count());

            return new ProductReviewSummaryDto
            {
                ProductId = productId,
                AverageRating = averageRating,
                TotalReviews = reviews.Count(),
                RatingDistribution = ratingDistribution
            };
        }

        // ✅ FIXED: Proper filtering logic
        public async Task<PagedResultDto<ReviewDto>> GetFilteredReviewsAsync(ReviewFilterDto filter, int pageNumber, int pageSize)
        {
            // Start with base query (non-deleted) and include navigation props for UI mapping
            IQueryable<Review> query = _reviewRepository.GetAll(q =>
                q.Include(r => r.User)
                 .Include(r => r.Product)
            ).Where(r => !r.IsDeleted);

            // Apply status filter
            if (filter != null)
            {
                // ✅ FIXED: Proper status filtering logic
                if (filter.IsApproved.HasValue)
                {
                    if (filter.IsApproved.Value)
                    {
                        // Approved reviews: IsApproved = true AND IsActive = true
                        query = query.Where(r => r.IsApproved == true && r.IsActive == true);
                    }
                    else
                    {
                        // For non-approved, we need to check if it's pending or rejected
                        // This will be handled by additional filtering logic below
                        query = query.Where(r => r.IsApproved == false);
                    }
                }

                // ✅ NEW: Add specific filtering for pending/rejected based on IsActive
                if (filter.IsPending.HasValue && filter.IsPending.Value)
                {
                    // Pending: IsApproved = false AND IsActive = true
                    query = query.Where(r => r.IsApproved == false && r.IsActive == true);
                }

                if (filter.IsRejected.HasValue && filter.IsRejected.Value)
                {
                    // Rejected: IsApproved = false AND IsActive = false
                    query = query.Where(r => r.IsApproved == false && r.IsActive == false);
                }

                if (filter.MinRating.HasValue)
                {
                    query = query.Where(r => r.Rating >= filter.MinRating.Value);
                }

                if (filter.MaxRating.HasValue)
                {
                    query = query.Where(r => r.Rating <= filter.MaxRating.Value);
                }

                if (filter.IsVerifiedPurchase.HasValue)
                {
                    query = query.Where(r => r.IsVerifiedPurchase == filter.IsVerifiedPurchase.Value);
                }

                if (filter.ProductId.HasValue)
                {
                    query = query.Where(r => r.ProductId == filter.ProductId.Value);
                }

                if (!string.IsNullOrWhiteSpace(filter.UserId))
                {
                    query = query.Where(r => r.UserId == filter.UserId);
                }

                if (filter.StartDate.HasValue)
                {
                    var s = filter.StartDate.Value.Date;
                    query = query.Where(r => r.CreatedAt >= s);
                }

                if (filter.EndDate.HasValue)
                {
                    var e = filter.EndDate.Value.Date.AddDays(1).AddTicks(-1);
                    query = query.Where(r => r.CreatedAt <= e);
                }

                if (!string.IsNullOrWhiteSpace(filter.SearchText))
                {
                    var term = filter.SearchText.Trim();
                    query = query.Where(r =>
                        EF.Functions.Like(r.Title, $"%{term}%") ||
                        EF.Functions.Like(r.ReviewText, $"%{term}%") ||
                        (r.Product != null && EF.Functions.Like(r.Product.Name, $"%{term}%")) ||
                        (r.User != null && EF.Functions.Like(r.User.Email, $"%{term}%"))
                    );
                }
            }

            // Get total count
            var totalCount = await query.CountAsync();

            var items = await query
                .OrderByDescending(r => r.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Map to DTO and return PagedResultDto
            var mapped = items.Adapt<IEnumerable<ReviewDto>>();

            return new PagedResultDto<ReviewDto>
            {
                Items = mapped,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
            };
        }

        public async Task SoftDeleteAsync(Guid id)
        {
            var entity = await _reviewRepository.GetByIdAsync(id);
            if (entity == null) throw new KeyNotFoundException("Review not found.");

            // Example: extra logic before delete
            if (entity.IsApproved)
            {
                throw new UnauthorizedAccessException("Only admins can delete approved reviews.");
            }

            entity.IsDeleted = true;
            await _reviewRepository.UpdateAsync(entity);
        }

        public async Task<bool> CanUserReviewProductAsync(string userId, Guid productId)
        {
            try
            {
                // Check if user has already reviewed this product
                if (await _reviewRepository.HasUserReviewedProductAsync(userId, productId))
                    return false;

                // Check if user has purchased this product
                var userOrders = await _orderRepository.GetOrdersByUserIdAsync(userId);
                var hasOrderedProduct = userOrders
                    .Where(o => o.OrderStatus == OrderStatus.Delivered)
                    .SelectMany(o => o.OrderItems)
                    .Any(oi => oi.Variant.ProductId == productId);

                return hasOrderedProduct;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if user {UserId} can review product {ProductId}", userId, productId);
                return false;
            }
        }

        // ✅ FIXED: Proper statistics calculation
        public async Task<ReviewStatsDto> GetReviewStatsAsync()
        {
            try
            {
                var totalReviews = await _repository.CountAsync(r => !r.IsDeleted);
                var approvedReviews = await _repository.CountAsync(r => r.IsApproved && !r.IsDeleted);
                var pendingReviews = await _repository.CountAsync(r => !r.IsApproved && r.IsActive && !r.IsDeleted);
                // ✅ FIXED: Proper rejected count - IsApproved = false AND IsActive = false
                var rejectedReviews = await _repository.CountAsync(r => !r.IsApproved && !r.IsActive && !r.IsDeleted);
                var verifiedPurchases = await _repository.CountAsync(r => r.IsVerifiedPurchase && !r.IsDeleted);

                var allApprovedReviews = await _repository.GetAll()
                    .Where(r => r.IsApproved && !r.IsDeleted)
                    .ToListAsync();
                var averageRating = allApprovedReviews.Any()
                    ? allApprovedReviews.Average(r => r.Rating)
                    : 0.0;

                return new ReviewStatsDto
                {
                    TotalReviews = totalReviews,
                    ApprovedReviews = approvedReviews,
                    PendingReviews = pendingReviews,
                    RejectedReviews = rejectedReviews,
                    AverageRating = Math.Round(averageRating, 2),
                    VerifiedPurchases = verifiedPurchases
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting review statistics");
                return new ReviewStatsDto();
            }
        }

        public override async Task ValidateCreateAsync(ReviewCreateDto createDto)
        {
            if (createDto.Rating < 1 || createDto.Rating > 5)
                throw new ArgumentException("Rating must be between 1 and 5");

            if (string.IsNullOrWhiteSpace(createDto.Title))
                throw new ArgumentException("Review title is required");

            if (string.IsNullOrWhiteSpace(createDto.ReviewText))
                throw new ArgumentException("Review comment is required");

            // Additional validation
            if (createDto.Title.Length > 200)
                throw new ArgumentException("Review title cannot exceed 200 characters");

            if (!string.IsNullOrEmpty(createDto.ReviewText) && createDto.ReviewText.Length > 1000)
                throw new ArgumentException("Review text cannot exceed 1000 characters");

            await Task.CompletedTask;
        }

        public override async Task ValidateUpdateAsync(Guid id, ReviewUpdateDto updateDto)
        {
            var existingReview = await _repository.GetByIdAsync(id);
            if (existingReview == null)
                throw new KeyNotFoundException($"Review with ID {id} not found");

            if (updateDto.Rating.HasValue && (updateDto.Rating < 1 || updateDto.Rating > 5))
                throw new ArgumentException("Rating must be between 1 and 5");

            if (!string.IsNullOrEmpty(updateDto.Title) && updateDto.Title.Length > 200)
                throw new ArgumentException("Review title cannot exceed 200 characters");

            if (!string.IsNullOrEmpty(updateDto.ReviewText) && updateDto.ReviewText.Length > 1000)
                throw new ArgumentException("Review text cannot exceed 1000 characters");

            await Task.CompletedTask;
        }

        public override async Task BeforeCreateAsync(Review entity)
        {
            entity.CreatedAt = DateTime.UtcNow;
            entity.UpdatedAt = DateTime.UtcNow;
            await Task.CompletedTask;
        }

        public override async Task BeforeUpdateAsync(Review entity)
        {
            entity.UpdatedAt = DateTime.UtcNow;
            await Task.CompletedTask;
        }

        public override async Task AfterCreateAsync(Review entity)
        {
            _logger.LogInformation("New review created: ID {ReviewId} for Product {ProductId} by User {UserId}",
                entity.Id, entity.ProductId, entity.UserId);
            await Task.CompletedTask;
        }

        public override async Task AfterUpdateAsync(Review entity)
        {
            _logger.LogInformation("Review updated: ID {ReviewId}", entity.Id);
            await Task.CompletedTask;
        }
    }
}