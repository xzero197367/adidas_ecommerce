using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adidas.DTOs.Main.Product_Variant_DTOs
{
    public class UpdateProductVariantDto
    {
        [Required, MaxLength(50)]
        public string Color { get; set; } = string.Empty;

        [Required, MaxLength(20)]
        public string Size { get; set; } = string.Empty;

        [Required, Range(0, int.MaxValue)]
        public int StockQuantity { get; set; }

        public decimal PriceAdjustment { get; set; } = 0;
        public string? ColorHex { get; set; }
        public int SortOrder { get; set; }
    }
}
