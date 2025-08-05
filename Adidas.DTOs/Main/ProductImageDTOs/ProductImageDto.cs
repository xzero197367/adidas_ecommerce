using Adidas.DTOs.Common_DTOs;

namespace Adidas.DTOs.Main.ProductImageDTOs
{
    public class ProductImageDto: BaseDto
    {
        public Guid Id { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public string AltText { get; set; } = string.Empty;
        public int SortOrder { get; set; }
        public bool IsPrimary { get; set; }
        public Guid ProductId { get; set; }
        public Guid? VariantId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }
    }
}
