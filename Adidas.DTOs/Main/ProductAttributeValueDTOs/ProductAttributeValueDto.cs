using Adidas.DTOs.Common_DTOs;
using Adidas.DTOs.Main.Product_DTOs;
using Adidas.DTOs.Main.ProductAttributeDTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adidas.DTOs.Main.ProductAttributeValueDTOs
{
    public class ProductAttributeValueDto: BaseDto
    {
        public Guid Id { get; set; }
        public string Value { get; set; } = string.Empty;
        public Guid ProductId { get; set; }
        public Guid AttributeId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }

        // Navigation properties as DTOs
        public ProductAttributeDto? Attribute { get; set; }
        public ProductDto? Product { get; set; }
    }
}
