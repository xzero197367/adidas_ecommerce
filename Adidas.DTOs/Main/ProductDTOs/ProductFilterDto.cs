using Models.People;

namespace Adidas.DTOs.Main.ProductDTOs
{
    public class ProductFilterDto
    {
        public Guid? CategoryId { get; set; }
        public Guid? BrandId { get; set; }
        public Gender? Gender { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public bool? IsOnSale { get; set; }
        public bool? InStock { get; set; }
        public string? SearchTerm { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? SortBy { get; set; }
        public bool SortDescending { get; set; } = false;

        // Properties for featured products
        public bool? IsFeatured { get; set; }
        public bool? IsActive { get; set; }

        // Date filters
        public DateTime? CreatedAfter { get; set; }
        public DateTime? CreatedBefore { get; set; }

        // Stock filters
        public int? MinStock { get; set; }
        public int? MaxStock { get; set; }
    }
}
