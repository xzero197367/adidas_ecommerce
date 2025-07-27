using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adidas.DTOs.Feature.ShoppingCartDTOS
{
    public class ShoppingCartItemDto
    {
        public Guid UserId { get; set; }
        public Guid ProductVariantId { get; set; }
        public Guid ProductId { get; set; }
        public string ProductName { get; set; }
        public string ProductSku { get; set; }
        public string VariantColor { get; set; }
        public string VariantSize { get; set; }
        public string VariantSku { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal SalePrice { get; set; }
        public decimal TotalPrice { get; set; }
        public int AvailableStock { get; set; }
        public bool IsAvailable { get; set; }
        public string ProductImageUrl { get; set; }

        // Related data
        //public ProductDto Product { get; set; }
        //public ProductVariantDto ProductVariant { get; set; }
    }
}
