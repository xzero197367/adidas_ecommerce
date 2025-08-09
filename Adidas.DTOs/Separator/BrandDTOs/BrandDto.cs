
using Adidas.DTOs.Common_DTOs;
using Adidas.DTOs.Main.ProductDTOs;

namespace Adidas.DTOs.Separator.Brand_DTOs
{
    public class BrandDto : BaseDto
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? LogoUrl { get; set; }
        public string? Website { get; set; }
        public int SortOrder { get; set; }

        public ICollection<ProductDto> Products { get; set; } = new List<ProductDto>();

        public int ProductCount => Products.Count;
    }
}
