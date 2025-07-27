using Adidas.DTOs.Operation.ReviewDTOs.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adidas.DTOs.Operation.ReviewDTOs.Result
{
    public class UserReviewSummaryDto
    {
        public string UserId { get; set; }
        public int TotalReviews { get; set; }
        public double AverageRatingGiven { get; set; }
        public int VerifiedPurchaseReviews { get; set; }
        public IEnumerable<ReviewDto> RecentReviews { get; set; } = new List<ReviewDto>();
    }
}
