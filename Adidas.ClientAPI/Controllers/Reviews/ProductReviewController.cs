using Adidas.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace Adidas.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class ProductReviewController : ControllerBase
    {
        private readonly ProductReviewService _productReviewService;

        public ProductReviewController(ProductReviewService productReviewService)
        {
            _productReviewService = productReviewService ?? throw new ArgumentNullException(nameof(productReviewService));
        }

        /// <summary>
        /// Get reviews for a specific product with pagination and summary
        /// </summary>
        /// <param name="productId">The product ID to get reviews for</param>
        /// <param name="pageNumber">Page number (default: 1)</param>
        /// <param name="pageSize">Page size (default: 10, max: 50)</param>
        /// <param name="includeUnapproved">Include unapproved reviews (default: false) - Admin only</param>
        /// <returns>Paginated product reviews with summary</returns>
        [HttpGet("{productId:guid}/reviews")]
        [ProducesResponseType(typeof(ProductReviewResultDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ProductReviewResultDto>> GetProductReviews(
            [FromRoute] Guid productId,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] bool includeUnapproved = false)
        {
            try
            {
                // Validate parameters
                if (productId == Guid.Empty)
                {
                    return BadRequest(new { error = "Invalid product ID" });
                }

                if (pageNumber < 1)
                {
                    return BadRequest(new { error = "Page number must be greater than 0" });
                }

                if (pageSize < 1 || pageSize > 50)
                {
                    return BadRequest(new { error = "Page size must be between 1 and 50" });
                }

                // Determine approval filter based on user role and parameter
                bool? isApproved = true; // Default: only approved reviews

                // If user wants unapproved reviews, check if they have permission
                if (includeUnapproved)
                {
                    // Check if user is admin/employee (you can implement your authorization logic here)
                    if (!await IsUserAuthorizedForUnapprovedReviews())
                    {
                        return Forbid("You don't have permission to view unapproved reviews");
                    }
                    isApproved = null; // Show all reviews (approved and unapproved)
                }

                var result = await _productReviewService.GetProductReviewsWithSummaryAsync(
                    productId, pageNumber, pageSize, isApproved);

                if (result.TotalCount == 0)
                {
                    return Ok(new ProductReviewResultDto
                    {
                        Reviews = new List<ProductReviewDto>(),
                        Summary = new ReviewSummaryDto(),
                        TotalCount = 0,
                        PageNumber = pageNumber,
                        PageSize = pageSize,
                        TotalPages = 0
                    });
                }

                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                // Log the exception here
                return StatusCode(StatusCodes.Status500InternalServerError, 
                    new { error = "An error occurred while retrieving product reviews" });
            }
        }

        /// <summary>
        /// Check if the current user is authorized to view unapproved reviews
        /// This is a placeholder method - implement your actual authorization logic here
        /// </summary>
        /// <returns>True if authorized, false otherwise</returns>
        private async Task<bool> IsUserAuthorizedForUnapprovedReviews()
        {
            // Implement your authorization logic here
            // For example, check if user has Admin or Employee role
            
            // Option 1: Check user claims
            if (User.IsInRole("Admin") || User.IsInRole("Employee"))
            {
                return true;
            }

            // Option 2: Check specific claims
            if (User.HasClaim("Permission", "ViewUnapprovedReviews"))
            {
                return true;
            }

            // Option 3: If you're using a more complex authorization system
            // you might inject an authorization service and check permissions there

            return false;
        }
    }

    // Additional response models for better API documentation
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public T? Data { get; set; }
        public string? Message { get; set; }
        public List<string>? Errors { get; set; }

        public static ApiResponse<T> SuccessResponse(T data, string? message = null)
        {
            return new ApiResponse<T>
            {
                Success = true,
                Data = data,
                Message = message
            };
        }

        public static ApiResponse<T> ErrorResponse(string error)
        {
            return new ApiResponse<T>
            {
                Success = false,
                Errors = new List<string> { error }
            };
        }

        public static ApiResponse<T> ErrorResponse(List<string> errors)
        {
            return new ApiResponse<T>
            {
                Success = false,
                Errors = errors
            };
        }
    }
}