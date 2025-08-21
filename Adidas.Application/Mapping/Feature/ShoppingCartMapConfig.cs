using Adidas.DTOs.Feature.ShoppingCartDTOS;
using Adidas.Models.Feature;
using Mapster;

namespace Adidas.Application.Mapping.Feature;

public class ShoppingCartMapConfig
{
    public static void Configure()
    {
        // Get DTO to Model
        TypeAdapterConfig<ShoppingCartDto, ShoppingCart>.NewConfig()
            .IgnoreNullValues(true);

        // Model to Get DTO
        TypeAdapterConfig<ShoppingCart, ShoppingCartDto>.NewConfig()
            .Map(d => d.Variant, s => s.Variant)
            .Map(d => d.VariantId, s => s.VariantId)
            .Map(d => d.Quantity, s => s.Quantity)
            .Map(d => d.SalePrice, s => s.Variant != null && s.Variant.Product != null ? s.Variant.Product.SalePrice : 0)
            .Map(d => d.UnitPrice, s => s.Variant != null && s.Variant.Product != null ? s.Variant.Product.Price : 0)
            .PreserveReference(true); // Enable reference preservation for this mapping

        // Create DTO to Model
        TypeAdapterConfig<ShoppingCartCreateDto, ShoppingCart>.NewConfig()
            .IgnoreNullValues(true);

        // Update DTO to Model
        TypeAdapterConfig<ShoppingCartUpdateDto, ShoppingCart>.NewConfig()
            .IgnoreNullValues(true);

        // Model to Cart Summary
        TypeAdapterConfig<IEnumerable<ShoppingCartDto>, ShoppingCartSummaryDto>.NewConfig()
            .Map(d => d.ItemCount, s => s.Count())
            .Map(d => d.Subtotal, s => s.Sum(x => x.SalePrice * x.Quantity))
            .Map(d => d.HasUnavailableItems, s => s.Any(x => !x.IsAvailable))
            .Map(d => d.Items, s => s)
            .Map(d => d.TotalQuantity, s => s.Sum(x => x.Quantity))
            .Map(d => d.TotalAmount, s => s.Sum(x => x.SalePrice * x.Quantity))
            .Map(d => d.SavingsAmount, s => s.Sum(x => (x.UnitPrice - x.SalePrice) * x.Quantity))
            .Map(d => d.TaxAmount, s => 0m) // Placeholder: Replace with actual tax calculation
            .Map(d => d.ShippingCost, s => 0m) // Placeholder: Replace with actual shipping calculation
            .Map(d => d.UnavailableItems, s => s.Where(x => !x.IsAvailable))
            .PreserveReference(true);
    }
}