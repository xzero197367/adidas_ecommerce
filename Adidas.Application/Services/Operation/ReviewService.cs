using Adidas.Application.Contracts.RepositoriesContracts.Main;
using Adidas.Application.Contracts.RepositoriesContracts.Operation;
using Adidas.Application.Contracts.ServicesContracts.Operation;
using Adidas.DTOs.Common_DTOs;
using Adidas.DTOs.Operation.ReviewDTOs.Create;
using Adidas.DTOs.Operation.ReviewDTOs.Query;
using Adidas.DTOs.Operation.ReviewDTOs.Result;
using Adidas.DTOs.Operation.ReviewDTOs.Update;
using AutoMapper;
using Microsoft.Extensions.Logging;

namespace Adidas.Application.Services.Operation
{
    public class ReviewService : GenericService<Review, ReviewDto, CreateReviewDto, UpdateReviewDto>, IReviewService
    {
        private readonly IReviewRepository _reviewRepository;
        private readonly IOrderRepository _orderRepository;

        public ReviewService(
            IReviewRepository reviewRepository,
            IOrderRepository orderRepository,
            IMapper mapper,
            ILogger<ReviewService> logger)
            : base(reviewRepository, mapper, logger)
        {
            _reviewRepository = reviewRepository;
            _orderRepository = orderRepository;
        }

        public async Task<PagedResultDto<ReviewDto>> GetReviewsByProductIdAsync(Guid productId, int pageNumber, int pageSize)
        {
            var reviews = await _reviewRepository.GetReviewsByProductIdAsync(productId);
            var pagedReviews = reviews.Skip((pageNumber - 1) * pageSize).Take(pageSize);

            return new PagedResultDto<ReviewDto>
            {
                Items = _mapper.Map<IEnumerable<ReviewDto>>(pagedReviews),
                TotalCount = reviews.Count(),
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling((double)reviews.Count() / pageSize)
            };
        }

        public async Task<IEnumerable<ReviewDto>> GetReviewsByUserIdAsync(string userId)
        {
            var reviews = await _reviewRepository.GetReviewsByUserIdAsync(userId);
            return _mapper.Map<IEnumerable<ReviewDto>>(reviews);
        }

        public async Task<ReviewDto> CreateReviewAsync(CreateReviewDto createReviewDto)
        {
            if (!await CanUserReviewProductAsync(createReviewDto.UserId, createReviewDto.ProductId))
                throw new InvalidOperationException("User cannot review this product");

            var review = _mapper.Map<Review>(createReviewDto);
            review.IsApproved = false; // Reviews need approval

            var createdReview = await _repository.AddAsync(review);
            return _mapper.Map<ReviewDto>(createdReview);
        }

        public async Task<bool> ApproveReviewAsync(Guid reviewId)
        {
            var review = await _repository.GetByIdAsync(reviewId);
            if (review == null) return false;

            review.IsApproved = true;
            review.ReviewText = null;
            await _repository.UpdateAsync(review);
            return true;
        }

        public async Task<bool> RejectReviewAsync(Guid reviewId, string reason)
        {
            var review = await _repository.GetByIdAsync(reviewId);
            if (review == null) return false;

            review.IsApproved = false;
            review.ReviewText = reason;
            await _repository.UpdateAsync(review);
            return true;
        }

        public async Task<ProductReviewSummaryDto> GetProductReviewSummaryAsync(Guid productId)
        {
            var reviews = await _reviewRepository.GetReviewsByProductIdAsync(productId);
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

        protected override Task ValidateCreateAsync(CreateReviewDto createDto)
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
