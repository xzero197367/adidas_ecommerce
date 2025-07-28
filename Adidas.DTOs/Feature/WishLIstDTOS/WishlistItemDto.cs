using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adidas.DTOs.Feature.WishLIstDTOS
{
    public class WishlistItemDto
    {
        public Guid UserId { get; set; }
        public Guid ProductId { get; set; }
        public string ProductName { get; set; }
        public string ProductSku { get; set; }
        public decimal Price { get; set; }
        public decimal? SalePrice { get; set; }
        public string ImageUrl { get; set; }
        public bool IsAvailable { get; set; }
        public bool IsOnSale { get; set; }
        public decimal? SavingsAmount { get; set; }
        public bool NotifyWhenInStock { get; set; }

        
      //  public ProductDto Product { get; set; }
    }
}