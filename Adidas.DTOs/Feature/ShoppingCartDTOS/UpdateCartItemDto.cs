using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adidas.DTOs.Feature.ShoppingCartDTOS
{
    public class UpdateCartItemDto
    {
        [Required]
        public string UserId { get; set; }

        [Required]
        public Guid ProductVariantId { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }
    }
}
