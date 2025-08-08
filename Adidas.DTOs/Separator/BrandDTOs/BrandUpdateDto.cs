
using System.ComponentModel.DataAnnotations;
using Adidas.DTOs.CommonDTOs;

namespace Adidas.DTOs.Separator.Brand_DTOs
{
    public class BrandUpdateDto: BaseUpdateDto
    {
        [StringLength(100, ErrorMessage = "Brand name cannot exceed 100 characters")]
        public string Name { get; set; }

        [StringLength(500, ErrorMessage = "Logo URL cannot exceed 500 characters")]
        public string? LogoUrl { get; set; }

        public string? Description { get; set; }
    }
}

