using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adidas.DTOs.Operation.OrderDTOs.Create
{
    public class CreateOrderItemDto
    {
        [Required]
        public Guid VariantId { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
        public int Quantity { get; set; }

        [Required]
        public string ProductName { get; set; }

        public string VariantDetails { get; set; }
    }
}
