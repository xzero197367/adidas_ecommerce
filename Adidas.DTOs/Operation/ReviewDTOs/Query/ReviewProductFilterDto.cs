namespace Adidas.DTOs.Operation.ReviewDTOs.Query
{
    public class ReviewProductFilterDto : ReviewFilterDto
    {
        public string? ProductName { get; set; }   // ✅ New optional filter
    }
}
