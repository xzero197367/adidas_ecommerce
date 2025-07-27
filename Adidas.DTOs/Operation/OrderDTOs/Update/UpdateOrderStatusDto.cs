using Adidas.Models.Operation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adidas.DTOs.Operation.OrderDTOs.Update
{
    public class UpdateOrderStatusDto
    {
        [Required]
        public OrderStatus OrderStatus { get; set; }
        public string Notes { get; set; }
    }
}
