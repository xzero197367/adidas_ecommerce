
using System.ComponentModel.DataAnnotations;
using Adidas.DTOs.CommonDTOs;

namespace Adidas.DTOs.Separator.Category_DTOs
{
    public class CategoryUpdateDto: BaseUpdateDto
    {
        [Required]
        [StringLength(100, ErrorMessage = "Category name cannot exceed 100 characters")]
        public string Name { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "Slug cannot exceed 100 characters")]
        public string Slug { get; set; }

        public string? Description { get; set; }
        [Required]
        [StringLength(500, ErrorMessage = "Image URL cannot exceed 500 characters")]
        public string? ImageUrl { get; set; }

        public Guid? ParentCategoryId { get; set; }

        public int SortOrder { get; set; }
    }


}
