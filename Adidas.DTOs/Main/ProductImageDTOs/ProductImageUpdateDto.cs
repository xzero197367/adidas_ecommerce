
using System.ComponentModel.DataAnnotations;
using Adidas.DTOs.CommonDTOs;

namespace Adidas.DTOs.Main.ProductImageDTOs
{
    public class ProductImageUpdateDto: BaseUpdateDto
    {
        public string? ImageUrl { get; set; }
        public string? AltText { get; set; }
        public int? SortOrder { get; set; }
        public bool? IsPrimary { get; set; }
        public Guid? VariantId { get; set; }
    }
}
