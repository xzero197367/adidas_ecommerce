using System.ComponentModel.DataAnnotations;
using Adidas.DTOs.CommonDTOs;
using Microsoft.AspNetCore.Http;
using Adidas.Models.Separator;

namespace Adidas.DTOs.Separator.Category_DTOs
{
    public class CategoryUpdateDto : BaseUpdateDto
    {
        [Required]
        [StringLength(100, ErrorMessage = "Category name cannot exceed 100 characters")]
        public string Name { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "Slug cannot exceed 100 characters")]
        public string Slug { get; set; }

        [Required]
        public string? Description { get; set; }

        //[StringLength(500, ErrorMessage = "Image URL cannot exceed 500 characters")]
        public string? ImageUrl { get; set; }

        public IFormFile? ImageFile { get; set; }

        public Guid? ParentCategoryId { get; set; }

        public int SortOrder { get; set; }

        public bool IsActive { get; set; }

        [Required(ErrorMessage = "Category type is required")]
        public CategoryType? Type { get; set; }
    }
}