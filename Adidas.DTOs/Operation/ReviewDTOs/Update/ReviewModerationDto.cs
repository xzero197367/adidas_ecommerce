using Adidas.DTOs.Operation.ReviewDTOs.Query;
using Adidas.DTOs.Operation.ReviewDTOs.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adidas.DTOs.Operation.ReviewDTOs.Update
{
    public class ReviewModerationDto : ReviewDto
    {
        public string? ModerationNotes { get; set; }
        public DateTime? ModerationDate { get; set; }
        public string? ModeratedBy { get; set; }
        public ProductSummaryDto? Product { get; set; }
        public UserSummaryDto? User { get; set; }
    }
}
