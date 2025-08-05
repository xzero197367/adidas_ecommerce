
using System.ComponentModel.DataAnnotations;

namespace Adidas.DTOs.Main.ProductImageDTOs
{
    public class UpdateProductImageDto
    {
        [Url(ErrorMessage = "Please provide a valid image URL")]
        public string? ImageUrl { get; set; }

        public string? AltText { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Sort order must be a non-negative number")]
        public int? SortOrder { get; set; }

        public bool? IsPrimary { get; set; }

        public Guid? VariantId { get; set; }
    }
}
