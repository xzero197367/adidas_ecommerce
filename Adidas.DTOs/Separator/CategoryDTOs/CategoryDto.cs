using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Adidas.DTOs.Common_DTOs;
using Adidas.DTOs.Main.Product_DTOs;

namespace Adidas.DTOs.Separator.Category_DTOs
{
    public class CategoryDto : BaseDto
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public Guid? ParentCategoryId { get; set; }
        public int SortOrder { get; set; }
        public string? MetaTitle { get; set; }
        public string? MetaDescription { get; set; }

        public CategoryDto? ParentCategory { get; set; }
        public ICollection<CategoryDto> SubCategories { get; set; } = new List<CategoryDto>();
        public ICollection<ProductDto> Products { get; set; } = new List<ProductDto>();

        public bool HasSubCategories => SubCategories.Any();
        public int ProductCount => Products.Count;
    }
}
