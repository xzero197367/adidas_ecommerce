
using Adidas.DTOs.Operation.PaymentDTOs.Result;

namespace Adidas.DTOs.Operation.OrderDTOs.Result
{
    public class PagedOrderResultDto
    {
        public List<OrderSummaryDto> Orders { get; set; } = new();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
        public bool HasPrevious => PageNumber > 1;
        public bool HasNext => PageNumber < TotalPages;
    }
}
