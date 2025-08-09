
using System.ComponentModel.DataAnnotations;

namespace Adidas.DTOs.Feature.ShoppingCartDTOS
{
    public class ShoppingCartCreateDto
    {
        [Required]
        public string UserId { get; set; }

        [Required]
        public Guid ProductVariantId { get; set; }

        [Required]
        public int Quantity { get; set; }
    }
}
