using Adidas.Application.Contracts.RepositoriesContracts.Main;
using Adidas.Application.Contracts.RepositoriesContracts.Operation;
using Adidas.Application.Contracts.RepositoriesContracts.People;
using Adidas.Application.Contracts.ServicesContracts.Operation;
using Adidas.Application.Contracts.ServicesContracts.Operation.Review;
using Adidas.DTOs.Operation.ReviewDTOs.Create;
using Adidas.DTOs.Operation.ReviewDTOs.Query;
using Adidas.DTOs.Operation.ReviewDTOs.Result;
using Adidas.DTOs.Operation.ReviewDTOs.Update;
using AutoMapper;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adidas.Application.Services.Operation
{
    public class ReviewService :
      IReviewQueryService,
        IReviewService,
     IReviewAnalyticsService,
     IReviewModerationService
    {
        private readonly IReviewRepository _reviewRepository;
        private readonly IProductRepository _productRepository;
        private readonly IUserRepository _userRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<ReviewService> _logger;

        public ReviewService(
            IReviewRepository reviewRepository,
            IProductRepository productRepository,
            IUserRepository userRepository,
            IOrderRepository orderRepository,
            IMapper mapper,
            ILogger<ReviewService> logger)
        {
            _reviewRepository = reviewRepository;
            _productRepository = productRepository;
            _userRepository = userRepository;
            _orderRepository = orderRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ReviewDto?> ApproveReviewAsync(Guid reviewId, string moderatorId)
        {
            var review = await _reviewRepository.GetByIdAsync(reviewId);
            if (review == null)
                return null;

            review.IsApproved = true;
            review.UpdatedAt = DateTime.UtcNow;
            review.AddedById = moderatorId;

            await _reviewRepository.UpdateAsync(review);

            _logger.LogInformation("Review approved by moderator: {ReviewId}, {ModeratorId}", reviewId, moderatorId);
            return _mapper.Map<ReviewDto>(review);
        }

        public async Task<bool> BulkApproveReviewsAsync(IEnumerable<Guid> reviewIds, string moderatorId)
        {
            foreach (var reviewId in reviewIds)
            {
                await ApproveReviewAsync(reviewId, moderatorId);
            }

            _logger.LogInformation("Bulk approved {Count} reviews by moderator: {ModeratorId}",
                reviewIds.Count(), moderatorId);
            return true;
        }

        public async Task<bool> BulkRejectReviewsAsync(IEnumerable<Guid> reviewIds, string moderatorId, string reason)
        {
            foreach (var reviewId in reviewIds)
            {
                await RejectReviewAsync(reviewId, moderatorId, reason);
            }

            _logger.LogInformation("Bulk rejected {Count} reviews by moderator: {ModeratorId}",
                reviewIds.Count(), moderatorId);
            return true;
        }

        public async Task<ReviewDto> CreateReviewAsync(CreateReviewDto createReviewDto)
        {
            var product = await _productRepository.GetByIdAsync(createReviewDto.ProductId);
            if (product == null)
                throw new ArgumentException($"Product with ID {createReviewDto.ProductId} not found");

            var user = await _userRepository.GetByIdAsync(createReviewDto.UserId);
            if (user == null)
                throw new ArgumentException($"User with ID {createReviewDto.UserId} not found");

            var hasReviewed = await _reviewRepository.HasUserReviewedProductAsync(createReviewDto.UserId, createReviewDto.ProductId);
            if (hasReviewed)
                throw new InvalidOperationException("User has already reviewed this product");

            var review = _mapper.Map<Review>(createReviewDto);
            review.IsApproved = false;


            var createdReview = await _reviewRepository.AddAsync(review);

            return _mapper.Map<ReviewDto>(createdReview);
        }
    


        public async Task<bool> DeleteReviewAsync(Guid id)
        {
            var review = await _reviewRepository.GetByIdAsync(id);
            if (review == null)
                return false;

            await _reviewRepository.SoftDeleteAsync(id);
            return true;
        }

        public async Task<IEnumerable<ReviewDto>> GetApprovedReviewsAsync(Guid productId)
        {
            var reviews = await _reviewRepository.GetApprovedReviewsAsync(productId);
            return _mapper.Map<IEnumerable<ReviewDto>>(reviews);
        }

        public async Task<double> GetAverageRatingAsync(Guid productId)
        {
            return await _reviewRepository.GetAverageRatingAsync(productId);
        }

        public async Task<IEnumerable<ReviewModerationDto>> GetPendingReviewsAsync()
        {
            var reviews = await _reviewRepository.GetPendingReviewsAsync();
            return _mapper.Map<IEnumerable<ReviewModerationDto>>(reviews);
        }

        public async Task<ProductReviewSummaryDto> GetProductReviewSummaryAsync(Guid productId)
        {
            var reviews = await _reviewRepository.GetApprovedReviewsAsync(productId);
            var reviewList = reviews.ToList();

            var ratingDistribution = reviewList
                .GroupBy(r => r.Rating)
                .ToDictionary(g => g.Key, g => g.Count());

            var recentReviews = reviewList
                .OrderByDescending(r => r.CreatedAt)
                .Take(5);

            return new ProductReviewSummaryDto
            {
                ProductId = productId,
                TotalReviews = reviewList.Count,
                AverageRating = reviewList.Any() ? reviewList.Average(r => r.Rating) : 0,
                RatingDistribution = ratingDistribution,
                RecentReviews = _mapper.Map<IEnumerable<ReviewDto>>(recentReviews),
                VerifiedPurchaseCount = reviewList.Count(r => r.IsVerifiedPurchase)
            };
        }

        public async Task<Dictionary<int, int>> GetRatingDistributionAsync(Guid productId)
        {
            var reviews = await _reviewRepository.GetApprovedReviewsAsync(productId);
            return reviews
                .GroupBy(r => r.Rating)
                .ToDictionary(g => g.Key, g => g.Count());
        }

        public async Task<IEnumerable<ReviewDto>> GetRecentReviewsAsync(int count = 10)
        {
            var reviews = await _reviewRepository.FindAsync(r => r.IsApproved && !r.IsDeleted);
            var recentReviews = reviews
                .OrderByDescending(r => r.CreatedAt)
                .Take(count);
            return _mapper.Map<IEnumerable<ReviewDto>>(recentReviews);
        }

        public async Task<ReviewDto?> GetReviewByIdAsync(Guid id)
        {
            var review = await _reviewRepository.GetByIdAsync(id);
            return review != null ? _mapper.Map<ReviewDto>(review) : null;
        }

        public async Task<IEnumerable<ReviewDto>> GetReviewsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            var reviews = await _reviewRepository.FindAsync(r =>
                               r.CreatedAt >= startDate && r.CreatedAt <= endDate && !r.IsDeleted);
            return _mapper.Map<IEnumerable<ReviewDto>>(reviews);
        }

        public async Task<IEnumerable<ReviewDto>> GetReviewsByProductIdAsync(Guid productId)
        {
            var reviews = await _reviewRepository.GetReviewsByProductIdAsync(productId);
            return _mapper.Map<IEnumerable<ReviewDto>>(reviews);
        }

        public async Task<IEnumerable<ReviewDto>> GetReviewsByRatingAsync(Guid productId, int rating)
        {
            var reviews = await _reviewRepository.FindAsync(r =>
                               r.ProductId == productId && r.Rating == rating && r.IsApproved && !r.IsDeleted);
            return _mapper.Map<IEnumerable<ReviewDto>>(reviews);
        }

        public async Task<IEnumerable<ReviewDto>> GetReviewsByUserIdAsync(string userId)
        {
            var reviews = await _reviewRepository.GetReviewsByUserIdAsync(userId);
            return _mapper.Map<IEnumerable<ReviewDto>>(reviews);
        }

        public async Task<PagedReviewDto> GetReviewsPagedAsync(Guid productId, int pageNumber, int pageSize, bool? isApproved = null)
        {
            var (reviews, totalCount) = await _reviewRepository.GetReviewsPagedAsync(productId, pageNumber, pageSize, isApproved);
            var reviewDtos = _mapper.Map<IEnumerable<ReviewDto>>(reviews);
            var averageRating = await _reviewRepository.GetAverageRatingAsync(productId);

            return new PagedReviewDto
            {
                Reviews = reviewDtos,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling((double)totalCount / pageSize),
                HasNextPage = pageNumber * pageSize < totalCount,
                HasPreviousPage = pageNumber > 1,
                AverageRating = averageRating
            };
        }

        public async Task<PagedReviewDto> GetReviewsPagedWithFiltersAsync(int pageNumber, int pageSize, ReviewFilterDto filters)
        {
            var productId = filters.ProductId ?? Guid.Empty;
            var (reviews, totalCount) = await _reviewRepository.GetReviewsPagedAsync(productId, pageNumber, pageSize, filters.IsApproved);
            var reviewDtos = _mapper.Map<IEnumerable<ReviewDto>>(reviews);

            return new PagedReviewDto
            {
                Reviews = reviewDtos,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling((double)totalCount / pageSize),
                HasNextPage = pageNumber * pageSize < totalCount,
                HasPreviousPage = pageNumber > 1
            };
        }

        public async Task<IEnumerable<ReviewDto>> GetReviewsRequiringModerationAsync()
        {
            var pendingReviews = await _reviewRepository.GetPendingReviewsAsync();
            return _mapper.Map<IEnumerable<ReviewDto>>(pendingReviews);
                }

        public async Task<ReviewStatsDto> GetReviewStatsAsync(Guid? productId = null)
        {
            IEnumerable<Review> reviews;

            if (productId.HasValue)
            {
                reviews = await _reviewRepository.GetReviewsByProductIdAsync(productId.Value);
            }
            else
            {
                reviews = await _reviewRepository.GetAllAsync();
                reviews = reviews.Where(r => !r.IsDeleted);
            }

            var totalReviews = reviews.Count();
            var approvedReviews = reviews.Count(r => r.IsApproved);
            var pendingReviews = totalReviews - approvedReviews;
            var verifiedPurchaseReviews = reviews.Count(r => r.IsVerifiedPurchase);

            var ratingDistribution = reviews
                .GroupBy(r => r.Rating)
                .ToDictionary(g => g.Key, g => g.Count());

            var averageRating = reviews.Any() ? reviews.Average(r => r.Rating) : 0;

            return new ReviewStatsDto
            {
                TotalReviews = totalReviews,
                ApprovedReviews = approvedReviews,
                PendingReviews = pendingReviews,
                VerifiedPurchaseReviews = verifiedPurchaseReviews,
                AverageRating = averageRating,
                RatingDistribution = ratingDistribution,
                ApprovalRate = totalReviews > 0 ? (double)approvedReviews / totalReviews * 100 : 0,
                VerifiedPurchaseRate = totalReviews > 0 ? (double)verifiedPurchaseReviews / totalReviews * 100 : 0
            };
        }

        public async Task<IEnumerable<ReviewDto>> GetTopRatedReviewsAsync(Guid productId, int count = 10)
        {
            var reviews = await _reviewRepository.GetApprovedReviewsAsync(productId);
            var topRatedReviews = reviews
                .OrderByDescending(r => r.Rating)
                .ThenByDescending(r => r.CreatedAt)
                .Take(count);
            return _mapper.Map<IEnumerable<ReviewDto>>(topRatedReviews);
        }

        public async Task<ReviewStatsDto> GetUserReviewingStatsAsync(string userId)
        {
            var reviews = await _reviewRepository.GetReviewsByUserIdAsync(userId);
            var reviewList = reviews.ToList();

            var approvedReviews = reviewList.Count(r => r.IsApproved);
            var pendingReviews = reviewList.Count(r => !r.IsApproved);
            var verifiedPurchaseReviews = reviewList.Count(r => r.IsVerifiedPurchase);

            var ratingDistribution = reviewList
                .GroupBy(r => r.Rating)
                .ToDictionary(g => g.Key, g => g.Count());

            return new ReviewStatsDto
            {
                TotalReviews = reviewList.Count,
                ApprovedReviews = approvedReviews,
                PendingReviews = pendingReviews,
                VerifiedPurchaseReviews = verifiedPurchaseReviews,
                AverageRating = reviewList.Any() ? reviewList.Average(r => r.Rating) : 0,
                RatingDistribution = ratingDistribution,
                ApprovalRate = reviewList.Count > 0 ? (double)approvedReviews / reviewList.Count * 100 : 0,
                VerifiedPurchaseRate = reviewList.Count > 0 ? (double)verifiedPurchaseReviews / reviewList.Count * 100 : 0
            };
        }

        public async Task<UserReviewSummaryDto> GetUserReviewSummaryAsync(string userId)
        {
            var reviews = await _reviewRepository.GetReviewsByUserIdAsync(userId);
            var reviewList = reviews.ToList();

            var recentReviews = reviewList
                .OrderByDescending(r => r.CreatedAt)
                .Take(5);

            return new UserReviewSummaryDto
            {
                UserId = userId,
                TotalReviews = reviewList.Count,
                AverageRatingGiven = reviewList.Any() ? reviewList.Average(r => r.Rating) : 0,
                VerifiedPurchaseReviews = reviewList.Count(r => r.IsVerifiedPurchase),
                RecentReviews = _mapper.Map<IEnumerable<ReviewDto>>(recentReviews)
            };
        }

        public async Task<IEnumerable<ReviewDto>> GetVerifiedPurchaseReviewsAsync(Guid productId)
        {
            var reviews = await _reviewRepository.GetVerifiedPurchaseReviewsAsync(productId);
            return _mapper.Map<IEnumerable<ReviewDto>>(reviews);
        }

        public async Task<bool> HasUserReviewedProductAsync(string userId, Guid productId)
        {
            return await _reviewRepository.HasUserReviewedProductAsync(userId, productId);
        }

        public async Task<ReviewDto?> RejectReviewAsync(Guid reviewId, string moderatorId, string reason)
        {
            var review = await _reviewRepository.GetByIdAsync(reviewId);
            if (review == null)
                return null;

            review.IsApproved = false;
            review.UpdatedAt = DateTime.UtcNow;
            review.AddedById = moderatorId;

            await _reviewRepository.UpdateAsync(review);

            _logger.LogInformation("Review rejected by moderator: {ReviewId}, {ModeratorId}, Reason: {Reason}",
                reviewId, moderatorId, reason);
            return _mapper.Map<ReviewDto>(review);
        }

        public async Task<IEnumerable<ReviewDto>> SearchReviewsAsync(string searchTerm)
        {
            var reviews = await _reviewRepository.FindAsync(r =>
                               (r.Title != null && r.Title.Contains(searchTerm)) ||
                               (r.ReviewText != null && r.ReviewText.Contains(searchTerm)) &&
                               r.IsApproved && !r.IsDeleted);
            return _mapper.Map<IEnumerable<ReviewDto>>(reviews);
        }

        public async Task<ReviewDto?> UpdateReviewAsync(Guid id, UpdateReviewDto updateReviewDto)
        {
            var review = await _reviewRepository.GetByIdAsync(id);
            if (review == null)
                return null;

            _mapper.Map(updateReviewDto, review);
            review.UpdatedAt = DateTime.UtcNow;
            review.IsApproved = false; 

            await _reviewRepository.UpdateAsync(review);
     

            _logger.LogInformation("Review updated successfully: {ReviewId}", id);
            return _mapper.Map<ReviewDto>(review);
        }

        public async Task<ReviewDto?> UpdateReviewModerationAsync(Guid id, AdminUpdateReviewDto adminUpdateDto, string moderatorId)
        {
            var review = await _reviewRepository.GetByIdAsync(id);
            if (review == null)
                return null;

            review.IsApproved = adminUpdateDto.IsApproved;
            review.UpdatedAt = DateTime.UtcNow;
            review.AddedById = moderatorId;

            await _reviewRepository.UpdateAsync(review);

            _logger.LogInformation("Review moderation updated: {ReviewId}, {ModeratorId}", id, moderatorId);
            return _mapper.Map<ReviewDto>(review);
        }
    }
}
