using Adidas.DTOs.Operation.ReviewDTOs.Query;

namespace Adidas.DTOs.Operation.ReviewDTOs
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
