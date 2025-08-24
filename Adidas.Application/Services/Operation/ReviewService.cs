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
        public override async Task<OperationResult<ReviewDto>> UpdateAsync(ReviewUpdateDto updateDto)
        {
            try
            {
                var existingEntity = await _repository.GetByIdAsync(updateDto.Id);
                if (existingEntity == null)
                    throw new KeyNotFoundException($"Review with id {updateDto.Id} not found");

                await ValidateUpdateAsync(updateDto.Id, updateDto);

                // Apply updates manually
                if (updateDto.Rating.HasValue) existingEntity.Rating = updateDto.Rating.Value;
                if (updateDto.Title != null) existingEntity.Title = updateDto.Title;
                if (updateDto.ReviewText != null) existingEntity.ReviewText = updateDto.ReviewText;
                if (updateDto.IsVerifiedPurchase.HasValue) existingEntity.IsVerifiedPurchase = updateDto.IsVerifiedPurchase.Value;
                if (updateDto.IsApproved.HasValue) existingEntity.IsApproved = updateDto.IsApproved.Value;
                if (updateDto.IsActive.HasValue) existingEntity.IsActive = updateDto.IsActive.Value;

                await BeforeUpdateAsync(existingEntity);

                var updatedEntityEntry = await _repository.UpdateAsync(existingEntity);
                var updatedEntity = updatedEntityEntry.Entity;
                await _repository.SaveChangesAsync();

                // Re-fetch to ensure the latest state
                updatedEntity = await _repository.GetByIdAsync(updateDto.Id);

                updatedEntityEntry.State = EntityState.Detached;
                await AfterUpdateAsync(updatedEntity);

                return OperationResult<ReviewDto>.Success(updatedEntity.Adapt<ReviewDto>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating review with id {Id}", updateDto.Id);
                return OperationResult<ReviewDto>.Fail("Error updating review: " + ex.Message);
            }
        }



        public async Task<ReviewDto> CreateReviewAsync(ReviewCreateDto createReviewDto, string userId)
        {
            //if (!await CanUserReviewProductAsync(createReviewDto.UserId, createReviewDto.ProductId))
            //    throw new InvalidOperationException("User cannot review this product");

            await ValidateCreateAsync(createReviewDto);

            var review = createReviewDto.Adapt<Review>();
            review.UserId = userId;
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

        public async Task<(bool Success, string Message)> ApproveReviewAsync(Guid reviewId)
        {
            try
            {
                var review = await _reviewRepository.GetByIdAsync(reviewId);
                if (review == null) return (false, "Review not found");

                if (review.IsApproved) return (false, "Review is already approved");

                review.IsApproved = true;
                review.IsActive = true;
                review.UpdatedAt = DateTime.UtcNow;

                await _reviewRepository.UpdateAsync(review);
                await _reviewRepository.SaveChangesAsync();
                return (true, "Review approved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error approving review {ReviewId}", reviewId);
                return (false, "An error occurred while approving the review");
            }
        }

        // ✅ FIXED: Proper rejection logic and set RejectionReason
        public async Task<(bool Success, string Message)> RejectReviewAsync(Guid reviewId, string reason)
        {
            try
            {
                var review = await _reviewRepository.GetByIdAsync(reviewId);
                if (review == null) return (false, "Review not found");

                if (!review.IsActive) return (false, "Review is already rejected");

                // ✅ FIXED: Set IsApproved = false AND IsActive = false for rejection
                review.IsApproved = false;
                review.IsActive = false; // This marks it as rejected
                review.RejectionReason = reason; // ✅ FIXED: Set the rejection reason
                review.UpdatedAt = DateTime.UtcNow;

                await _reviewRepository.UpdateAsync(review);
                await _reviewRepository.SaveChangesAsync();
                return (true, "Review rejected successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rejecting review {ReviewId}", reviewId);
                return (false, "An error occurred while rejecting the review");
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
            IQueryable<Review> query = _reviewRepository.GetAll()
                .Include(r => r.User)
                .Include(r => r.Product)
                .Where(r => !r.IsDeleted);

            _logger.LogInformation("Applying filter: {@Filter}", filter);

            if (filter != null)
            {
                // Apply status filters with mutual exclusivity
                bool hasStatusFilter = false;
                if (filter.IsApproved.HasValue && filter.IsApproved.Value)
                {
                    query = query.Where(r => r.IsApproved && r.IsActive);
                    hasStatusFilter = true;
                }
                if (filter.IsPending.HasValue && filter.IsPending.Value && !hasStatusFilter)
                {
                    query = query.Where(r => !r.IsApproved && r.IsActive);
                    hasStatusFilter = true;
                }
                if (filter.IsRejected.HasValue && filter.IsRejected.Value && !hasStatusFilter)
                {
                    query = query.Where(r => !r.IsApproved && !r.IsActive);
                    hasStatusFilter = true;
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

            var totalCount = await query.CountAsync();
            var items = await query
                .OrderByDescending(r => r.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var mapped = items.Adapt<IEnumerable<ReviewDto>>();

            return new PagedResultDto<ReviewDto>
            {
                Items = mapped.ToList(),
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