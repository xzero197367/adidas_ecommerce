using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adidas.DTOs.Operation.ReviewDTOs.Result
{
    public class ReviewStatsDto
    {
        //public int TotalReviews { get; set; }
        //public int ApprovedReviews { get; set; }
        //public int PendingReviews { get; set; }
        //public int RejectedReviews { get; set; }

        //public int VerifiedPurchaseReviews { get; set; }
        //public double AverageRating { get; set; }
        //public Dictionary<int, int> RatingDistribution { get; set; } = new();
        //public double ApprovalRate { get; set; }
        //public double VerifiedPurchaseRate { get; set; }
        public int TotalReviews { get; set; }
        public int ApprovedReviews { get; set; }
        public int PendingReviews { get; set; }
        public int RejectedReviews { get; set; }
        public double AverageRating { get; set; }
        public int VerifiedPurchases { get; set; }
    }

}
