
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Adidas.Application.Contracts.ServicesContracts.Operation;
using Adidas.DTOs.Operation.ReviewDTOs;
using Adidas.DTOs.Operation.ReviewDTOs.Query;
using Adidas.DTOs.Operation.ReviewDTOs.Result;
using Adidas.DTOs.Common_DTOs;
using System.Security.Claims;

namespace Adidas.ClientAPI.Controllers.Operation
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ReviewsController : ControllerBase
    {
        private readonly IReviewService _reviewService;
        private readonly ILogger<ReviewsController> _logger;

        public ReviewsController(IReviewService reviewService, ILogger<ReviewsController> logger)
        {
            _reviewService = reviewService;
            _logger = logger;
        }

        #region CRUD Operations

        /// <summary>
        /// Get all reviews with pagination
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<PagedResultDto<ReviewDto>>> GetReviews(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                var result = await _reviewService.GetPagedAsync(pageNumber, pageSize);
                if (!result.IsSuccess)
                    return BadRequest(result.ErrorMessage);

                return Ok(result.Data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting reviews");
                return StatusCode(500, "An error occurred while retrieving reviews");
            }
        }

        /// <summary>
        /// Get review by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<ReviewDto>> GetReview(Guid id)
        {
            try
            {
                var result = await _reviewService.GetByIdAsync(id);
                if (!result.IsSuccess || result.Data == null)
                    return NotFound($"Review with ID {id} not found");

                return Ok(result.Data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting review {ReviewId}", id);
                return StatusCode(500, "An error occurred while retrieving the review");
            }
        }

        /// <summary>
        /// Create a new review - Modified for easier testing
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<ReviewDto>> CreateReview([FromBody] ReviewCreateDto createReviewDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(currentUserId))
                    return Unauthorized("User not authenticated");

                createReviewDto.UserId = currentUserId;

   

                var createdReview = await _reviewService.CreateReviewAsync(createReviewDto);
                return CreatedAtAction(nameof(GetReview), new { id = createdReview.Id }, createdReview);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating review");
                return StatusCode(500, "An error occurred while creating the review");
            }
        }

        /// <summary>
        /// Update a review - Enhanced debugging for ownership checks
        /// </summary>
        [HttpPut("{id}")]
 
        public async Task<ActionResult<ReviewDto>> UpdateReview(Guid id, [FromBody] ReviewUpdateDto updateReviewDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var existingReviewResult = await _reviewService.GetByIdAsync(id);
                if (!existingReviewResult.IsSuccess || existingReviewResult.Data == null)
                    return NotFound($"Review with ID {id} not found");

                // Enhanced user validation with debugging info
                var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(currentUserId))
                    return Unauthorized("User not authenticated");

                // ✅ ENHANCED: Better error messaging for debugging
                if (existingReviewResult.Data.UserId != currentUserId)
                {
                    _logger.LogWarning("User {CurrentUserId} tried to update review {ReviewId} owned by {ReviewUserId}",
                        currentUserId, id, existingReviewResult.Data.UserId);

                    return StatusCode(403, new
                    {
                        message = "You can only update your own reviews",
                        currentUserId = currentUserId,
                        reviewOwnerId = existingReviewResult.Data.UserId,
                        reviewId = id
                    });
                }

                // Set the ID in the update DTO
                updateReviewDto.Id = id;
               

                var updateResult = await _reviewService.UpdateAsync(updateReviewDto);

                if (!updateResult.IsSuccess)
                    return BadRequest(updateResult.ErrorMessage);

                return Ok(updateResult.Data);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating review {ReviewId}", id);
                return StatusCode(500, "An error occurred while updating the review");
            }
        }

        /// <summary>
        /// Delete a review - Enhanced debugging for ownership checks
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<ActionResult> DeleteReview(Guid id)
        {
            try
            {
                var existingReviewResult = await _reviewService.GetByIdAsync(id);
                if (!existingReviewResult.IsSuccess || existingReviewResult.Data == null)
                    return NotFound($"Review with ID {id} not found");

                // Enhanced user validation with debugging info
                var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(currentUserId))
                    return Unauthorized("User not authenticated");

                // ✅ ENHANCED: Better error messaging for debugging
                if (existingReviewResult.Data.UserId != currentUserId)
                {
                    _logger.LogWarning("User {CurrentUserId} tried to delete review {ReviewId} owned by {ReviewUserId}",
                        currentUserId, id, existingReviewResult.Data.UserId);

                    return StatusCode(403, new
                    {
                        message = "You can only delete your own reviews",
                        currentUserId = currentUserId,
                        reviewOwnerId = existingReviewResult.Data.UserId,
                        reviewId = id
                    });
                }

                var result = await _reviewService.DeleteAsync(id);
                if (!result.IsSuccess)
                    return BadRequest(result.ErrorMessage);

                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting review {ReviewId}", id);
                return StatusCode(500, "An error occurred while deleting the review");
            }
        }

        #endregion

        #region Product Reviews

        /// <summary>
        /// Get reviews for a specific product
        /// </summary>
        [HttpGet("product/{productId}")]
        public async Task<ActionResult<PagedResultDto<ReviewDto>>> GetProductReviews(
            Guid productId,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                var result = await _reviewService.GetReviewsByProductIdAsync(productId, pageNumber, pageSize);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting reviews for product {ProductId}", productId);
                return StatusCode(500, "An error occurred while retrieving product reviews");
            }
        }

        /// <summary>
        /// Get review summary for a specific product
        /// </summary>
        [HttpGet("product/{productId}/summary")]
        public async Task<ActionResult<ProductReviewSummaryDto>> GetProductReviewSummary(Guid productId)
        {
            try
            {
                var summary = await _reviewService.GetProductReviewSummaryAsync(productId);
                return Ok(summary);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting review summary for product {ProductId}", productId);
                return StatusCode(500, "An error occurred while retrieving product review summary");
            }
        }

        /// <summary>
        /// Check if current user can review a product
        /// </summary>
        [HttpGet("product/{productId}/can-review")]
        [Authorize]
        public async Task<ActionResult<bool>> CanUserReviewProduct(Guid productId)
        {
            try
            {
                var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(currentUserId))
                    return Unauthorized("User not authenticated");

                var canReview = await _reviewService.CanUserReviewProductAsync(currentUserId, productId);
                return Ok(canReview);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if user can review product {ProductId}", productId);
                return StatusCode(500, "An error occurred while checking review eligibility");
            }
        }

        #endregion

        #region User Reviews

        /// <summary>
        /// Get reviews by current user
        /// </summary>
        [HttpGet("my-reviews")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<ReviewDto>>> GetMyReviews()
        {
            try
            {
                var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(currentUserId))
                    return Unauthorized("User not authenticated");

                var reviews = await _reviewService.GetReviewsByUserIdAsync(currentUserId);
                return Ok(reviews);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting current user's reviews");
                return StatusCode(500, "An error occurred while retrieving your reviews");
            }
        }

        #endregion

        #region Public Filtering and Search

        /// <summary>
        /// Get filtered reviews with advanced filtering options
        /// </summary>
        [HttpPost("filter")]
        public async Task<ActionResult<PagedResultDto<ReviewDto>>> GetFilteredReviews(
            [FromBody] ReviewFilterDto filter,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                var result = await _reviewService.GetFilteredReviewsAsync(filter, pageNumber, pageSize);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting filtered reviews");
                return StatusCode(500, "An error occurred while filtering reviews");
            }
        }

        /// <summary>
        /// Get approved reviews by status
        /// </summary>
        [HttpGet("approved")]
        public async Task<ActionResult<PagedResultDto<ReviewDto>>> GetApprovedReviews(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                var filter = new ReviewFilterDto
                {
                    IsApproved = true
                };

                var result = await _reviewService.GetFilteredReviewsAsync(filter, pageNumber, pageSize);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting approved reviews");
                return StatusCode(500, "An error occurred while retrieving approved reviews");
            }
        }

        #endregion

        #region Health Check

        /// <summary>
        /// Health check endpoint
        /// </summary>
        [HttpGet("health")]
        public ActionResult GetHealth()
        {
            return Ok(new { Status = "Healthy", Timestamp = DateTime.UtcNow });
        }

        #endregion
    }

    /// <summary>
    /// DTO for review rejection
    /// </summary>
    public class ReviewRejectionDto
    {
        public string Reason { get; set; } = string.Empty;
    }
}