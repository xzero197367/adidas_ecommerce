


using Microsoft.AspNetCore.Mvc;
using Adidas.Application.Contracts.ServicesContracts.Operation;
using Adidas.DTOs.Operation.ReviewDTOs.Query;
using Adidas.DTOs.Common_DTOs;
using System.ComponentModel.DataAnnotations;
using Adidas.DTOs.Operation.ReviewDTOs.Result;

using Adidas.DTOs.Operation.ReviewDTOs;
using Microsoft.AspNetCore.Authorization;


namespace Adidas.Web.Controllers
{
    [Authorize(Policy = "EmployeeOrAdmin")]

    public class ReviewsController : Controller
    {
        private readonly IReviewService _reviewService;
        private readonly ILogger<ReviewsController> _logger;

        public ReviewsController(
            IReviewService reviewService,
            ILogger<ReviewsController> logger)
        {
            _reviewService = reviewService ?? throw new ArgumentNullException(nameof(reviewService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpGet]
        public async Task<IActionResult> Index(
            int page = 1,
            int pageSize = 10,
            string? status = null,
            string? searchTerm = null,
            int? rating = null,
            DateTime? startDate = null,
            DateTime? endDate = null)
        {
            try
            {
                var viewModel = new ReviewsIndexViewModel
                {
                    CurrentPage = page,
                    PageSize = pageSize,
                    Status = status,
                    SearchTerm = searchTerm,
                    Rating = rating,
                    StartDate = startDate,
                    EndDate = endDate
                };

                // Get filtered and paged reviews
                var pagedReviews = await GetFilteredReviewsAsync(viewModel);
                viewModel.Reviews = pagedReviews;

                // ✅ FIXED: Get review statistics using the service method
                var stats = await _reviewService.GetReviewStatsAsync();
                viewModel.Stats = stats;

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading reviews index page");
                TempData["Error"] = "An error occurred while loading reviews.";
                return View(new ReviewsIndexViewModel());
            }
        }

        [HttpGet]
        public async Task<IActionResult> FilterReviews(
            int draw = 1,
            int start = 0,
            int length = 10,
            string? status = null,
            string? searchValue = null,
            int? rating = null,
            DateTime? startDate = null,
            DateTime? endDate = null)
        {
            try
            {
                var pageNumber = (start / length) + 1;

                // ✅ FIXED: Use proper filtering with status
                var filter = new ReviewFilterDto
                {
                    MinRating = rating,
                    MaxRating = rating,
                    StartDate = startDate,
                    EndDate = endDate,
                    SearchText = searchValue
                };

                // Apply status filtering
                if (!string.IsNullOrWhiteSpace(status))
                {
                    var s = status.ToLowerInvariant();
                    switch (s)
                    {
                        case "pending":
                            filter.IsPending = true;
                            break;
                        case "approved":
                            filter.IsApproved = true;
                            break;
                        case "rejected":
                            filter.IsRejected = true;
                            break;
                    }
                }

                var pagedReviews = await _reviewService.GetFilteredReviewsAsync(filter, pageNumber, length);

                var response = new
                {
                    draw,
                    recordsTotal = pagedReviews.TotalCount,
                    recordsFiltered = pagedReviews.TotalCount,
                    data = pagedReviews.Items.Select(r => new
                    {
                        id = r.Id,
                        customer = new
                        {
                            name = r.UserId,
                            email = r.UserId
                        },
                        product = new
                        {
                            name = "Product Name",
                            id = r.ProductId
                        },
                        rating = r.Rating,
                        review = r.ReviewText,
                        date = r.CreatedAt.ToString("MMM dd, yyyy"),
                        status = r.IsApproved ? "Approved" : (r.IsActive ? "Pending" : "Rejected"),
                        isApproved = r.IsApproved,
                        isVerifiedPurchase = r.IsVerifiedPurchase,
                        isDeleted = r.IsDeleted,
                        isActive = r.IsActive
                    })
                };

                return Json(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting reviews data for DataTable");
                return Json(new
                {
                    draw,
                    recordsTotal = 0,
                    recordsFiltered = 0,
                    data = Array.Empty<object>()
                });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetStats()
        {
            try
            {
                // ✅ FIXED: Use the service method for statistics
                var stats = await _reviewService.GetReviewStatsAsync();
                return Json(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting review statistics");
                return Json(new ReviewStatsDto());
            }
        }

        [HttpGet("details/{id:guid}")]
        public async Task<IActionResult> Details(Guid id)
        {
            try
            {
                var review = await _reviewService.GetByIdAsync(id);
                if (review == null || !review.IsSuccess)
                {
                    return NotFound();
                }

                return View(review.Data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading review details for ID: {ReviewId}", id);
                TempData["Error"] = "Review not found.";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [Route("Reviews/approve/{id:guid}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(Guid id)
        {
            try
            {
                var result = await _reviewService.ApproveReviewAsync(id);
                if (result)
                {
                    TempData["Success"] = "Review approved successfully.";
                    _logger.LogInformation("Review {ReviewId} approved by {User}", id, User.Identity?.Name);
                }
                else
                {
                    TempData["Error"] = "Failed to approve review.";
                }

                return Json(new { success = result, message = result ? "Review approved" : "Failed to approve review" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error approving review {ReviewId}", id);
                return Json(new { success = false, message = "An error occurred while approving the review." });
            }
        }

        [HttpPost]
        [Route("Reviews/reject/{id:guid}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(Guid id, [FromBody] RejectReviewRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return Json(new { success = false, message = "Invalid request data." });
                }

                var result = await _reviewService.RejectReviewAsync(id, request.Reason);
                if (result)
                {
                    TempData["Success"] = "Review rejected successfully.";
                    _logger.LogInformation("Review {ReviewId} rejected by {User} with reason: {Reason}",
                        id, User.Identity?.Name, request.Reason);
                }
                else
                {
                    TempData["Error"] = "Failed to reject review.";
                }

                return Json(new { success = result, message = result ? "Review rejected" : "Failed to reject review" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rejecting review {ReviewId}", id);
                return Json(new { success = false, message = "An error occurred while rejecting the review." });
            }
        }

        [HttpPost("bulk-action")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BulkAction([FromBody] BulkActionRequest request)
        {
            try
            {
                if (!ModelState.IsValid || !request.ReviewIds.Any())
                {
                    return Json(new { success = false, message = "Invalid request data." });
                }

                var successCount = 0;
                var failCount = 0;

                foreach (var reviewId in request.ReviewIds)
                {
                    bool result = request.Action.ToLower() switch
                    {
                        "approve" => await _reviewService.ApproveReviewAsync(reviewId),
                        "reject" => await _reviewService.RejectReviewAsync(reviewId, request.Reason ?? "Bulk rejection"),
                        _ => false
                    };

                    if (result) successCount++;
                    else failCount++;
                }

                var message = $"Bulk action completed. {successCount} successful, {failCount} failed.";
                _logger.LogInformation("Bulk {Action} performed by {User}. Success: {Success}, Failed: {Failed}",
                    request.Action, User.Identity?.Name, successCount, failCount);

                return Json(new { success = true, message, successCount, failCount });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error performing bulk action");
                return Json(new { success = false, message = "An error occurred during bulk operation." });
            }
        }

        [HttpDelete("{id:guid}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var result = await _reviewService.DeleteAsync(id);
                if (result.IsSuccess)
                {
                    _logger.LogInformation("Review {ReviewId} deleted by {User}", id, User.Identity?.Name);
                    return Json(new { success = true, message = "Review deleted successfully." });
                }

                return Json(new { success = false, message = "Review not found or could not be deleted." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting review {ReviewId}", id);
                return Json(new { success = false, message = "An error occurred while deleting the review." });
            }
        }

        // ✅ FIXED: Updated GetFilteredReviewsAsync method with proper status mapping
        private async Task<PagedResultDto<ReviewDto>> GetFilteredReviewsAsync(ReviewsIndexViewModel viewModel)
        {
            // Map viewModel to ReviewFilterDto
            var filter = new ReviewFilterDto
            {
                MinRating = viewModel.Rating,
                MaxRating = viewModel.Rating,
                IsVerifiedPurchase = null, // if you have a verifiedOnly checkbox bind it to the viewModel and set here
                ProductId = null,
                UserId = null,
                StartDate = viewModel.StartDate,
                EndDate = viewModel.EndDate,
                SearchText = viewModel.SearchTerm
            };

            // ✅ FIXED: Proper status mapping for pending, approved, and rejected
            if (!string.IsNullOrWhiteSpace(viewModel.Status))
            {
                var s = viewModel.Status.ToLowerInvariant();
                switch (s)
                {
                    case "pending":
                        filter.IsPending = true;        // IsApproved = false AND IsActive = true
                        break;
                    case "approved":
                        filter.IsApproved = true;       // IsApproved = true AND IsActive = true
                        break;
                    case "rejected":
                        filter.IsRejected = true;       // IsApproved = false AND IsActive = false
                        break;
                }
            }

            var result = await _reviewService.GetFilteredReviewsAsync(filter, viewModel.CurrentPage, viewModel.PageSize);
            return result;
        }

        // Request models for API endpoints
        public class RejectReviewRequest
        {
            [Required]
            [StringLength(500)]
            public string Reason { get; set; } = string.Empty;
        }

        public class BulkActionRequest
        {
            [Required]
            public List<Guid> ReviewIds { get; set; } = new();

            [Required]
            public string Action { get; set; } = string.Empty;

            public string? Reason { get; set; }
        }

        // ViewModel for the Index view
        public class ReviewsIndexViewModel
        {
            public PagedResultDto<ReviewDto> Reviews { get; set; } = new();
            public ReviewStatsDto Stats { get; set; } = new();
            public int CurrentPage { get; set; } = 1;
            public int PageSize { get; set; } = 10;
            public string? Status { get; set; }
            public string? SearchTerm { get; set; }
            public int? Rating { get; set; }
            public DateTime? StartDate { get; set; }
            public DateTime? EndDate { get; set; }
        }
        // ✅ Test Data Creation Script
        // Add this method to your ReviewsController or create a separate test controller

        [HttpPost]
        public async Task<IActionResult> CreateTestReviews()
        {
            try
            {
                // Get a sample product and user (adjust IDs as needed)
                var sampleProductId = Guid.NewGuid(); // Replace with actual product ID
                var sampleUserId = "test-user-id"; // Replace with actual user ID

                var testReviews = new List<ReviewCreateDto>
        {
            // Approved Review
            new ReviewCreateDto
            {
                Rating = 5,
                Title = "Great Product - Approved",
                ReviewText = "This is an approved review for testing",
                IsVerifiedPurchase = true,
                ProductId = sampleProductId,
                UserId = sampleUserId
            },
            // Pending Review  
            new ReviewCreateDto
            {
                Rating = 4,
                Title = "Good Product - Pending",
                ReviewText = "This is a pending review for testing",
                IsVerifiedPurchase = true,
                ProductId = sampleProductId,
                UserId = sampleUserId + "2"
            },
            // Another Pending Review
            new ReviewCreateDto
            {
                Rating = 3,
                Title = "Average Product - Pending",
                ReviewText = "This is another pending review for testing",
                IsVerifiedPurchase = false,
                ProductId = sampleProductId,
                UserId = sampleUserId + "3"
            }
        };

                // Create the test reviews
                foreach (var reviewDto in testReviews)
                {
                    var result = await _reviewService.CreateAsync(reviewDto);
                    if (result.IsSuccess)
                    {
                        // For the first review, approve it
                        if (reviewDto.Title.Contains("Approved"))
                        {
                            await _reviewService.ApproveReviewAsync(result.Data.Id);
                        }
                        // For one of the pending reviews, reject it
                        else if (reviewDto.Title.Contains("Average"))
                        {
                            await _reviewService.RejectReviewAsync(result.Data.Id, "Test rejection for demo purposes");
                        }
                    }
                }

                return Json(new { success = true, message = "Test reviews created successfully!" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating test reviews");
                return Json(new { success = false, message = "Error creating test reviews: " + ex.Message });
            }
        }
    }
}