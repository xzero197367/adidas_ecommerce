using Adidas.DTOs.Common_DTOs;

namespace Adidas.DTOs.Main.ProductAttributeDTOs
{
    public class ProductAttributeDto : BaseDto
    {
        public string Name { get; set; } = string.Empty;
        public string DataType { get; set; } = string.Empty;
        public bool IsFilterable { get; set; }
        public bool IsRequired { get; set; }
        public int SortOrder { get; set; }
    }
}
