using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adidas.DTOs.Main.ProductAttributeValueDTOs
{
    public class UpdateProductAttributeValueDto
    {
        [StringLength(500, MinimumLength = 1, ErrorMessage = "Value must be between 1 and 500 characters")]
        public string? Value { get; set; }

        public Guid? AttributeId { get; set; }
    }
}
