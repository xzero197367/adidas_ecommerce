using Adidas.DTOs.Operation.ReviewDTOs.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adidas.DTOs.Operation.ReviewDTOs.Result
{
    public class ProductReviewSummaryDto
    {
        public Guid ProductId { get; set; }
        public int TotalReviews { get; set; }
        public double AverageRating { get; set; }
        public Dictionary<int, int> RatingDistribution { get; set; } = new();
        public IEnumerable<ReviewDto> RecentReviews { get; set; } = new List<ReviewDto>();
        public int VerifiedPurchaseCount { get; set; }
    }

}
