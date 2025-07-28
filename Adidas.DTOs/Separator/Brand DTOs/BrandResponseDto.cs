using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adidas.DTOs.Separator.Brand_DTOs
{
    public class BrandResponseDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string? LogoUrl { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public int ProductCount { get; set; }
    }
}
