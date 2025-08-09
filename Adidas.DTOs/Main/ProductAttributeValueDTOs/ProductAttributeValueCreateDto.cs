
using System.ComponentModel.DataAnnotations;

namespace Adidas.DTOs.Main.ProductAttributeValueDTOs
{
    public class ProductAttributeValueCreateDto
    {
        [Required]
        [StringLength(500, MinimumLength = 1, ErrorMessage = "Value must be between 1 and 500 characters")]
        public required string Value { get; set; }
        [Required]
        public required Guid ProductId { get; set; }
        [Required]
        public required Guid AttributeId { get; set; }
    }
}
