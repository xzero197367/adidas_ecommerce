
using System.ComponentModel.DataAnnotations;

namespace Adidas.DTOs.Main.ProductAttributeDTOs
{
    public class ProductAttributeCreateDto
    {
        [Required]
        public string Name { get; set; }
        [Required]
        public string DataType { get; set; }
        [Required]
        public bool IsFilterable { get; set; }
        [Required]
        public bool IsRequired { get; set; }
        [Required]
        public int SortOrder { get; set; }
    }
}
