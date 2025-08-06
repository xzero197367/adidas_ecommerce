using Adidas.Models.Operation;
using System.ComponentModel.DataAnnotations;

namespace Adidas.DTOs.Operation.OrderDTOs.Update
{
    public class UpdateOrderStatusDto
    {
        [Required]
        public OrderStatus OrderStatus { get; set; }
        public string Notes { get; set; }
    }
}
