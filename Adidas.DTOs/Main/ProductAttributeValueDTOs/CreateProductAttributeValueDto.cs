using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adidas.DTOs.Main.ProductAttributeValueDTOs
{
    public class CreateProductAttributeValueDto
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
