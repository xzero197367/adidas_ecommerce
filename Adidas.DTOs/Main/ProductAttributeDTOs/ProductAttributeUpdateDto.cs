

using Adidas.DTOs.CommonDTOs;

namespace Adidas.DTOs.Main.ProductAttributeDTOs
{
    public class ProductAttributeUpdateDto: BaseUpdateDto
    {
        public string? Name { get; set; }
        public string? DataType { get; set; }
        public bool? IsFilterable { get; set; }
        public bool? IsRequired { get; set; }
        public int? SortOrder { get; set; }
    }
}
