using Adidas.DTOs.Common_DTOs;
using Adidas.DTOs.Main.ProductAttributeValueDTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adidas.DTOs.Main.ProductAttributeDTOs
{
    public class ProductAttributeDto : BaseDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string DataType { get; set; } = string.Empty;
        public bool IsFilterable { get; set; }
        public bool IsRequired { get; set; }
        public int SortOrder { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }

        // Include attribute values if needed
        public List<ProductAttributeValueDto> AttributeValues { get; set; } = new List<ProductAttributeValueDto>();
    }
}
