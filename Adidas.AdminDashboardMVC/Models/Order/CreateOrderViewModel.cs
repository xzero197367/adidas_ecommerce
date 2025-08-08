
using Adidas.DTOs.Main.Product_Variant_DTOs;
using Adidas.DTOs.Operation.OrderDTOs.Create;
using Adidas.DTOs.People.Customer_DTOs;

namespace Adidas.AdminDashboardMVC.Models.Order
{
    public class CreateOrderViewModel : OrderCreateDto
    {
        public List<CustomerDto> Customers { get; set; } = new();
        public List<ProductVariantDto> ProductVariants { get; set; } = new();
        public List<OrderItemCreateDto> SelectedItems { get; set; } = new();
    }
}
