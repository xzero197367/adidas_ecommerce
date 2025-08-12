

using Microsoft.AspNetCore.Mvc;
using Adidas.Application.Contracts.ServicesContracts.Operation;
using Adidas.DTOs.Operation.ReviewDTOs.Query;
using Adidas.DTOs.Common_DTOs;
using System.ComponentModel.DataAnnotations;
using Adidas.DTOs.Operation.ReviewDTOs.Result;
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

                // Get review statistics
                var stats = await GetReviewStatsAsync();
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

        // إضافة الـ Action الجديد للـ DataTable
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

                // استخدام الـ Repository method الموجود
                var pagedReviews = await _reviewService.GetPagedAsync(pageNumber, length);

                // تحويل البيانات للـ DataTable format
                var response = new
                {
                    draw,
                    recordsTotal = pagedReviews.Data.TotalCount,
                    recordsFiltered = pagedReviews.Data.TotalCount,
                    data = pagedReviews.Data.Items.Select(r => new
                    {
                        id = r.Id,
                        customer = new
                        {
                            name = r.UserId, // يمكنك تحسين هذا بإضافة User navigation
                            email = r.UserId
                        },
                        product = new
                        {
                            name = "Product Name", // يحتاج تحسين بإضافة Product navigation
                            id = r.ProductId
                        },
                        rating = r.Rating,
                        review = r.ReviewText,
                        //date = r.CreatedAt?.ToString("MMM dd, yyyy") ?? "N/A",
                        //date = r != null ? r.CreatedAt.ToString("MMM dd, yyyy") : "N/A",
                        date = r != null ? ((DateTime)r.CreatedAt).ToString("MMM dd, yyyy") : "N/A",


                        status = r.IsApproved ? "Approved" : "Pending",
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
                var stats = await GetReviewStatsAsync();
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
                if (review == null)
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

        private async Task<ReviewStatsDto> GetReviewStatsAsync()
        {
            try
            {
                var totalReviews = await _reviewService.CountAsync();
                var approvedReviews = await _reviewService.CountAsync(r => r.IsApproved);
                var pendingReviews = totalReviews.Data - approvedReviews.Data;
                // الرفض يحتاج لتعديل في الـ Repository للتمييز بين Rejected و Pending
                var rejectedReviews = 0; // سنحسن هذا لاحقاً

                return new ReviewStatsDto
                {
                    TotalReviews = totalReviews.Data,
                    ApprovedReviews = approvedReviews.Data,
                    PendingReviews = pendingReviews,
                    RejectedReviews = rejectedReviews
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating review statistics");
                return new ReviewStatsDto();
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

        //private async Task<PagedResultDto<ReviewDto>> GetFilteredReviewsAsync(ReviewsIndexViewModel viewModel)
        //{
        //    // يمكنك تحسين هذه الدالة لإضافة الفلاتر
        //    var  result = await _reviewService.GetPagedAsync(viewModel.CurrentPage, viewModel.PageSize);
        //    return result.Data;
        //}
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

            // If you receive status as string in viewModel.Status, map to IsApproved/IsActive:
            if (!string.IsNullOrWhiteSpace(viewModel.Status))
            {
                var s = viewModel.Status.ToLowerInvariant();
                switch (s)
                {
                    case "pending":
                        filter.IsApproved = false; // pending = not approved and active
                                                   // note: you could add an IsActive flag to filter if needed
                        break;
                    case "approved":
                        filter.IsApproved = true;
                        break;
                    case "rejected":
                        filter.IsApproved = false;
                        // indicate inactive in repository/service if you want to distinguish
                        break;
                }
            }

            var result = await _reviewService.GetFilteredReviewsAsync(filter, viewModel.CurrentPage, viewModel.PageSize);
            return result;
        }

        //    private async Task<ReviewStatsDto> GetReviewStatsAsync()
        //    {
        //        try
        //        {
        //            var totalReviews = await _reviewService.CountAsync();
        //            var approvedReviews = await _reviewService.CountAsync(r => r.IsApproved);
        //            var pendingReviews = totalReviews.Data - approvedReviews.Data;
        //            // الرفض يحتاج لتعديل في الـ Repository للتمييز بين Rejected و Pending
        //            var rejectedReviews = 0; // سنحسن هذا لاحقاً

        //            return new ReviewStatsDto
        //            {
        //                TotalReviews = totalReviews.Data,
        //                ApprovedReviews = approvedReviews.Data,
        //                PendingReviews = pendingReviews,
        //                RejectedReviews = rejectedReviews
        //            };
        //        }
        //        catch (Exception ex)
        //        {
        //            _logger.LogError(ex, "Error calculating review statistics");
        //            return new ReviewStatsDto();
        //        }
        //    }
        //}


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
    }
}