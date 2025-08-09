using System.ComponentModel.DataAnnotations;

namespace Adidas.DTOs.Operation.OrderDTOs.Create
{
    public class OrderItemCreateDto
    {
        [Required]
        public Guid VariantId { get; set; }
        
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
        public int Quantity { get; set; }
        
        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Unit price must be greater than 0")]
        public decimal UnitPrice { get; set; }
    }
}
