using Adidas.Models.Operation;

namespace Adidas.DTOs.Operation.OrderDTOs.Query
{
    public class OrderQueryDto
    {
        public string UserId { get; set; }
        public OrderStatus? Status { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string OrderNumber { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string SortBy { get; set; } = "OrderDate";
        public bool SortDescending { get; set; } = true;
    }
}
