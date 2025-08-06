using Adidas.Models.Operation;

namespace Adidas.DTOs.Operation.OrderDTOs.Update
{
    public class UpdateOrderDto
    {
        public OrderStatus? OrderStatus { get; set; }
        public string ShippingAddress { get; set; }
        public string BillingAddress { get; set; }
        public string Notes { get; set; }
        public DateTime? ShippedDate { get; set; }
        public DateTime? DeliveredDate { get; set; }
    }

}
