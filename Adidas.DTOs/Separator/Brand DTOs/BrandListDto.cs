

namespace Adidas.DTOs.Separator.Brand_DTOs
{
    public class BrandDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string? LogoUrl { get; set; }
        public bool IsActive { get; set; }
        public int ProductCount { get; set; }
    }
}
