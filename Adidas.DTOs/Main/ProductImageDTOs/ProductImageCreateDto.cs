
using System.ComponentModel.DataAnnotations;

namespace Adidas.DTOs.Main.ProductImageDTOs
{
    public class ProductImageCreateDto
    {
        [Required]
        [Url(ErrorMessage = "Please provide a valid image URL")]
        public required string ImageUrl { get; set; }

        public string AltText { get; set; } = string.Empty;

        [Range(0, int.MaxValue, ErrorMessage = "Sort order must be a non-negative number")]
        public int SortOrder { get; set; }

        [Required]
        public required bool IsPrimary { get; set; }

        [Required]
        public required Guid ProductId { get; set; }

        public Guid? VariantId { get; set; }
    }
}
