using Adidas.DTOs.Operation.ReviewDTOs.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adidas.DTOs.Operation.ReviewDTOs.Query
{
    public class ReviewWithDetailsDto : ReviewDto
    {
        public ProductSummaryDto? Product { get; set; }
        public UserSummaryDto? User { get; set; }
    }
}