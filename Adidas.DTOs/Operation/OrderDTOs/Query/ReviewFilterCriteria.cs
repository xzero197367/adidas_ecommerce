namespace Adidas.DTOs.Operation.ReviewDTOs.Query
{
    public class ReviewFilterCriteria
    {
        public string? Status { get; set; }
        public string? SearchTerm { get; set; }
        public int? Rating { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public bool? IsVerifiedPurchase { get; set; }
        public Guid? ProductId { get; set; }
        public string? UserId { get; set; }
        public string SortBy { get; set; } = "CreatedAt";
        public string SortDirection { get; set; } = "DESC";
    }
}