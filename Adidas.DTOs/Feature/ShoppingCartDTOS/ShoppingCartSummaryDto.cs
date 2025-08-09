
namespace Adidas.DTOs.Feature.ShoppingCartDTOS
{
    public class ShoppingCartSummaryDto
    {
        public string UserId { get; set; }
        public int ItemCount { get; set; }
        public int TotalQuantity { get; set; }
        public decimal Subtotal { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal ShippingCost { get; set; }
        public decimal TotalAmount { get; set; }
        
        public decimal SavingsAmount { get; set; }
        public bool HasUnavailableItems { get; set; }
        public IEnumerable<ShoppingCartDto> Items { get; set; }
        public IEnumerable<ShoppingCartDto> UnavailableItems { get; set; }
    }
}
