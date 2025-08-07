using Adidas.Application.Contracts.RepositoriesContracts.Operation;
using Adidas.Application.Contracts.ServicesContracts.Operation;
using Adidas.DTOs.Common_DTOs;
using Adidas.DTOs.CommonDTOs;
using Adidas.DTOs.Operation.ReviewDTOs;
using Adidas.DTOs.Operation.ReviewDTOs.Query;
using Adidas.DTOs.Operation.ReviewDTOs.Result;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Adidas.Application.Services.Operation
{
    public class ReviewService : GenericService<Review, ReviewDto, ReviewCreateDto, ReviewUpdateDto>,IReviewService
    {
        private readonly IReviewRepository _reviewRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly ILogger<ReviewService> _logger;

        public ReviewService(
            IReviewRepository reviewRepository,
            IOrderRepository orderRepository,
            ILogger<ReviewService> logger): base(reviewRepository, logger)
        {
            _reviewRepository = reviewRepository;
            _orderRepository = orderRepository;
            _logger = logger;
        }

        public async Task<OperationResult<PagedResultDto<ReviewDto>>> GetReviewsByProductIdAsync(Guid productId,
            int pageNumber, int pageSize)
        {
            try
            {
                var reviews = await _reviewRepository.GetReviewsByProductIdAsync(productId);
                var pagedReviews = reviews.Skip((pageNumber - 1) * pageSize).Take(pageSize);

                var result = new PagedResultDto<ReviewDto>
                {
                    Items = pagedReviews.Adapt<IEnumerable<ReviewDto>>(),
                    TotalCount = reviews.Count(),
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalPages = (int)Math.Ceiling((double)reviews.Count() / pageSize)
                };
                return OperationResult<PagedResultDto<ReviewDto>>.Success(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving reviews for product {ProductId}", productId);
                return OperationResult<PagedResultDto<ReviewDto>>.Fail("Error retrieving reviews for product");
            }
        }

        public async Task<OperationResult<IEnumerable<ReviewDto>>> GetReviewsByUserIdAsync(string userId)
        {
            try
            {
                var reviews = await _reviewRepository.GetReviewsByUserIdAsync(userId);
                return OperationResult<IEnumerable<ReviewDto>>.Success(reviews.Adapt<IEnumerable<ReviewDto>>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving reviews for user {UserId}", userId);
                return OperationResult<IEnumerable<ReviewDto>>.Fail("Error retrieving reviews for user");
            }
        }

        public async Task<OperationResult<ReviewDto>> CreateReviewAsync(ReviewCreateDto createReviewDto)
        {
            try
            {
                var canReview = await CanUserReviewProductAsync(createReviewDto.UserId, createReviewDto.ProductId);
                if (!canReview.Data)
                {
                    return OperationResult<ReviewDto>.Fail("You can't review this product");
                }

                var review = createReviewDto.Adapt<Review>();
                review.IsApproved = false; // Reviews need approval

                var createdReview = await _reviewRepository.AddAsync(review);
                await _reviewRepository.SaveChangesAsync();
                createdReview.State = EntityState.Detached;

                return OperationResult<ReviewDto>.Success(createdReview.Entity.Adapt<ReviewDto>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating review for product {ProductId}", createReviewDto.ProductId);
                return OperationResult<ReviewDto>.Fail("Error creating review for product");
            }
        }

        public async Task<OperationResult<bool>> ApproveReviewAsync(Guid reviewId)
        {
            try
            {
                var review = await _reviewRepository.GetByIdAsync(reviewId);
                if (review == null)
                {
                    return OperationResult<bool>.Fail("Review not found");
                }

                review.IsApproved = true;
                review.ReviewText = null;
                var result = await _reviewRepository.UpdateAsync(review);
                await _reviewRepository.SaveChangesAsync();
                result.State = EntityState.Detached;
                return OperationResult<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error approving review {ReviewId}", reviewId);
                return OperationResult<bool>.Fail("Error approving review");
            }
        }

        public async Task<OperationResult<bool>> RejectReviewAsync(Guid reviewId, string reason)
        {
            try
            {
                var review = await _reviewRepository.GetByIdAsync(reviewId);
                if (review == null)
                {
                    return OperationResult<bool>.Fail("Review not found");
                }

                review.IsApproved = false;
                review.ReviewText = reason;
                var result = await _reviewRepository.UpdateAsync(review);
                await _reviewRepository.SaveChangesAsync();
                result.State = EntityState.Detached;
                return OperationResult<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rejecting review {ReviewId}", reviewId);
                return OperationResult<bool>.Fail("Error rejecting review");
            }
        }

        public async Task<OperationResult<ProductReviewSummaryDto>> GetProductReviewSummaryAsync(Guid productId)
        {
            try
            {
                var reviews = await _reviewRepository.GetReviewsByProductIdAsync(productId);
                var averageRating = await _reviewRepository.GetAverageRatingAsync(productId);

                var ratingDistribution = reviews
                    .GroupBy(r => r.Rating)
                    .ToDictionary(g => g.Key, g => g.Count());

                var result = new ProductReviewSummaryDto
                {
                    ProductId = productId,
                    AverageRating = averageRating,
                    TotalReviews = reviews.Count(),
                    RatingDistribution = ratingDistribution
                };

                return OperationResult<ProductReviewSummaryDto>.Success(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving review summary for product {ProductId}", productId);
                return OperationResult<ProductReviewSummaryDto>.Fail("Error retrieving review summary for product");
            }
        }

        public async Task<OperationResult<bool>> CanUserReviewProductAsync(string userId, Guid productId)
        {
            try
            {
                // Check if user has already reviewed this product
                var hasUserReviewed = await _reviewRepository.HasUserReviewedProductAsync(userId, productId);
                if (hasUserReviewed)
                {
                    return OperationResult<bool>.Success(false);
                }

                // Check if user has purchased this product
                var userOrders = await _orderRepository.GetOrdersByUserIdAsync(userId);
                var hasOrderedProduct = userOrders
                    .Where(o => o.OrderStatus == OrderStatus.Delivered)
                    .SelectMany(o => o.OrderItems)
                    .Any(oi => oi.Variant.ProductId == productId);

                return OperationResult<bool>.Success(hasOrderedProduct);
            }catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if user can review product {ProductId}", productId);
                return OperationResult<bool>.Fail("Error checking if user can review product"); 
            }
        }

        protected  Task ValidateCreateAsync(ReviewCreateDto createDto)
        {
            if (createDto.Rating < 1 || createDto.Rating > 5)
                throw new ArgumentException("Rating must be between 1 and 5");

            if (string.IsNullOrWhiteSpace(createDto.Title))
                throw new ArgumentException("Review title is required");

            if (string.IsNullOrWhiteSpace(createDto.ReviewText))
                throw new ArgumentException("Review comment is required");

            return Task.CompletedTask;
        }
    }
}