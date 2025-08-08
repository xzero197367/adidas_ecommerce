//using Adidas.Application.Contracts.RepositoriesContracts.Main;
//using Adidas.Application.Contracts.RepositoriesContracts.Operation;
//using Adidas.Application.Contracts.RepositoriesContracts.People;
//using Adidas.Application.Contracts.ServicesContracts.Operation;
//using Adidas.DTOs.Common_DTOs;
//using Adidas.DTOs.Operation.ReviewDTOs.Create;
//using Adidas.DTOs.Operation.ReviewDTOs.Query;
//using Adidas.DTOs.Operation.ReviewDTOs.Result;
//using Adidas.DTOs.Operation.ReviewDTOs.Update;
//using AutoMapper;
//using Microsoft.Extensions.Logging;

//namespace Adidas.Application.Services.Operation
//{
//    public class ReviewService : GenericService<Review, ReviewDto, CreateReviewDto, UpdateReviewDto>, IReviewService
//    {
//        private readonly IReviewRepository _reviewRepository;
//        private readonly IOrderRepository _orderRepository;

//        public ReviewService(
//            IReviewRepository reviewRepository,
//            IOrderRepository orderRepository,
//            IMapper mapper,
//            ILogger<ReviewService> logger)
//            : base(reviewRepository, mapper, logger)
//        {
//            _reviewRepository = reviewRepository;
//            _orderRepository = orderRepository;
//        }

//        public async Task<PagedResultDto<ReviewDto>> GetReviewsByProductIdAsync(Guid productId, int pageNumber, int pageSize)
//        {
//            var reviews = await _reviewRepository.GetReviewsByProductIdAsync(productId);
//            var pagedReviews = reviews.Skip((pageNumber - 1) * pageSize).Take(pageSize);

//            return new PagedResultDto<ReviewDto>
//            {
//                Items = _mapper.Map<IEnumerable<ReviewDto>>(pagedReviews),
//                TotalCount = reviews.Count(),
//                PageNumber = pageNumber,
//                PageSize = pageSize,
//                TotalPages = (int)Math.Ceiling((double)reviews.Count() / pageSize)
//            };
//        }

//        public async Task<IEnumerable<ReviewDto>> GetReviewsByUserIdAsync(string userId)
//        {
//            var reviews = await _reviewRepository.GetReviewsByUserIdAsync(userId);
//            return _mapper.Map<IEnumerable<ReviewDto>>(reviews);
//        }

//        public async Task<ReviewDto> CreateReviewAsync(CreateReviewDto createReviewDto)
//        {
//            if (!await CanUserReviewProductAsync(createReviewDto.UserId, createReviewDto.ProductId))
//                throw new InvalidOperationException("User cannot review this product");

//            var review = _mapper.Map<Review>(createReviewDto);
//            review.IsApproved = false; // Reviews need approval

//            var createdReview = await _repository.AddAsync(review);
//            return _mapper.Map<ReviewDto>(createdReview);
//        }

//        public async Task<bool> ApproveReviewAsync(Guid reviewId)
//        {
//            var review = await _repository.GetByIdAsync(reviewId);
//            if (review == null) return false;

//            review.IsApproved = true;
//            review.ReviewText = null;
//            await _repository.UpdateAsync(review);
//            return true;
//        }

//        public async Task<bool> RejectReviewAsync(Guid reviewId, string reason)
//        {
//            var review = await _repository.GetByIdAsync(reviewId);
//            if (review == null) return false;

//            review.IsApproved = false;
//            review.ReviewText = reason;
//            await _repository.UpdateAsync(review);
//            return true;
//        }

//        public async Task<ProductReviewSummaryDto> GetProductReviewSummaryAsync(Guid productId)
//        {
//            var reviews = await _reviewRepository.GetReviewsByProductIdAsync(productId);
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

//        public async Task<bool> CanUserReviewProductAsync(string userId, Guid productId)
//        {
//            // Check if user has already reviewed this product
//            if (await _reviewRepository.HasUserReviewedProductAsync(userId, productId))
//                return false;

//            // Check if user has purchased this product
//            var userOrders = await _orderRepository.GetOrdersByUserIdAsync(userId);
//            var hasOrderedProduct = userOrders
//                .Where(o => o.OrderStatus == OrderStatus.Delivered)
//                .SelectMany(o => o.OrderItems)
//                .Any(oi => oi.Variant.ProductId == productId);

//            return hasOrderedProduct;
//        }

//        protected override Task ValidateCreateAsync(CreateReviewDto createDto)
//        {
//            if (createDto.Rating < 1 || createDto.Rating > 5)
//                throw new ArgumentException("Rating must be between 1 and 5");

//            if (string.IsNullOrWhiteSpace(createDto.Title))
//                throw new ArgumentException("Review title is required");

//            if (string.IsNullOrWhiteSpace(createDto.ReviewText))
//                throw new ArgumentException("Review comment is required");

//            return Task.CompletedTask;
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

        public async Task<bool> RejectReviewAsync(Guid reviewId, string reason)
        {
            try
            {
                var review = await _reviewRepository.GetByIdAsync(reviewId);
                if (review == null) return false;

                review.IsApproved = false;
                review.IsActive = false; // Mark as inactive to indicate rejection
                review.UpdatedAt = DateTime.UtcNow;
                // يمكنك إضافة حقل RejectReason إلى الـ model إذا كنت تريد حفظ السبب

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

        // Enhanced statistics method
        public async Task<ReviewStatsDto> GetReviewStatsAsync()
        {
            try
            {
                var totalReviews = await _repository.CountAsync(r => !r.IsDeleted);
                var approvedReviews = await _repository.CountAsync(r => r.IsApproved && !r.IsDeleted);
                var pendingReviews = await _reviewRepository.GetPendingReviewsCountAsync();
                var rejectedReviews = await _reviewRepository.GetRejectedReviewsCountAsync();
                var verifiedPurchases = await _repository.CountAsync(r => r.IsVerifiedPurchase && !r.IsDeleted);

                // Calculate average rating for all approved reviews
                var allApprovedReviews = await _repository.GetAll().Where(r => r.IsApproved && !r.IsDeleted).ToListAsync();
                var averageRating = allApprovedReviews.Any() ? allApprovedReviews.Average(r => r.Rating) : 0.0;

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