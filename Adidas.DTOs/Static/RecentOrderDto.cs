using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adidas.DTOs.Static
{
    public class RecentOrderDto
    {
        public Guid OrderId { get; set; }
        public string CustomerName { get; set; }
        public decimal TotalAmount { get; set; }
        public string OrderStatus { get; set; }
        public DateTime OrderDate { get; set; }
        public string StatusBadgeClass => OrderStatus switch
        {
            "Completed" => "badge-success",
            "Pending" => "badge-warning",
            "Processing" => "badge-info",
            "Cancelled" => "badge-danger",
            _ => "badge-secondary"
        };
    }
}
