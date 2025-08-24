//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace Adidas.DTOs.Operation.ReviewDTOs.Query
//{
//    public class ReviewFilterDto
//    {
//        public Guid? ProductId { get; set; }
//        public string? UserId { get; set; }
//        public int? MinRating { get; set; }
//        public int? MaxRating { get; set; }
//        public bool? IsApproved { get; set; }
//        public bool? IsVerifiedPurchase { get; set; }
//        public DateTime? StartDate { get; set; }
//        public DateTime? EndDate { get; set; }
//        public string? SearchText { get; set; }
//    }
//}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adidas.DTOs.Operation.ReviewDTOs.Query
{
    public class ReviewFilterDto
    {
        public Guid? ProductId { get; set; }
        public int? MinRating { get; set; }
        public int? MaxRating { get; set; }
        public bool? IsApproved { get; set; }
        public bool? IsVerifiedPurchase { get; set; }

        // ✅ NEW: Add specific flags for pending and rejected
        public bool? IsPending { get; set; }
        public bool? IsRejected { get; set; }

        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? SearchText { get; set; }
    }
}