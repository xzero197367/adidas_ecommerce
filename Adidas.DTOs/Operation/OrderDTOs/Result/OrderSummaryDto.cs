using Adidas.Models.Operation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adidas.DTOs.Operation.OrderDTOs.Result
{
    public class OrderSummaryDto
    {
        public Guid Id { get; set; }
        public string OrderNumber { get; set; }
        public OrderStatus OrderStatus { get; set; }
        public decimal TotalAmount { get; set; }
        public string Currency { get; set; }
        public DateTime OrderDate { get; set; }
        public string UserName { get; set; }
        public string UserEmail { get; set; }
        public int ItemCount { get; set; }
    }
}
