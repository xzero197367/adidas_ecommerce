
using Adidas.DTOs.CommonDTOs;

namespace Adidas.DTOs.Main.ProductAttributeValueDTOs
{
    public class ProductAttributeValueUpdateDto : BaseUpdateDto
    {
        public string? Value { get; set; }

        public Guid? ProductId { get; set; }

        public Guid? AttributeId { get; set; }
    }
}
