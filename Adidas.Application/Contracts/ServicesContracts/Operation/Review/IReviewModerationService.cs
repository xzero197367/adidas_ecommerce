using Adidas.DTOs.Operation.ReviewDTOs.Query;
using Adidas.DTOs.Operation.ReviewDTOs.Update;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adidas.Application.Contracts.ServicesContracts.Operation.Review
{
    public interface IReviewModerationService
    {
        Task<IEnumerable<ReviewModerationDto>> GetPendingReviewsAsync();
        Task<ReviewDto?> ApproveReviewAsync(Guid reviewId, string moderatorId);
        Task<ReviewDto?> RejectReviewAsync(Guid reviewId, string moderatorId, string reason);
        Task<IEnumerable<ReviewDto>> GetReviewsRequiringModerationAsync();
        Task<ReviewDto?> UpdateReviewModerationAsync(Guid id, AdminUpdateReviewDto adminUpdateDto, string moderatorId);
        Task<bool> BulkApproveReviewsAsync(IEnumerable<Guid> reviewIds, string moderatorId);
        Task<bool> BulkRejectReviewsAsync(IEnumerable<Guid> reviewIds, string moderatorId, string reason);

    }
}
