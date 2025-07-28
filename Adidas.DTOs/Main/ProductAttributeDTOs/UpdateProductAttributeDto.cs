using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adidas.DTOs.Main.ProductAttributeDTOs
{
    public class UpdateProductAttributeDto
    {
        [StringLength(100, MinimumLength = 1, ErrorMessage = "Name must be between 1 and 100 characters")]
        public string? Name { get; set; }

        [StringLength(50, MinimumLength = 1, ErrorMessage = "DataType must be between 1 and 50 characters")]
        public string? DataType { get; set; }

        public bool? IsFilterable { get; set; }

        public bool? IsRequired { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Sort order must be a non-negative number")]
        public int? SortOrder { get; set; }
    }
}
