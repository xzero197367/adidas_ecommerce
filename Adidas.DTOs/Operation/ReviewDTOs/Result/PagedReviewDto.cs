using Adidas.DTOs.Operation.ReviewDTOs.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adidas.DTOs.Operation.ReviewDTOs.Result
{
    public class PagedReviewDto
    {
        public IEnumerable<ReviewDto> Reviews { get; set; } = new List<ReviewDto>();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public bool HasNextPage { get; set; }
        public bool HasPreviousPage { get; set; }
        public double AverageRating { get; set; }
    }

}
