using Adidas.DTOs.Feature.ShoppingCartDTOS;
using Adidas.Models.Feature;
using Mapster;

namespace Adidas.Application.Mapping.Feature;

public class ShoppingCartMapConfig
{
    public static void Configure()
    {
        // Get DTO to Model
        TypeAdapterConfig.GlobalSettings.NewConfig<ShoppingCartDto, ShoppingCart>()
            .IgnoreNullValues(true);

        // Model to Get DTO
        TypeAdapterConfig.GlobalSettings.NewConfig<ShoppingCart, ShoppingCartDto>()
            .Map(d => d.Variant, s => s.Variant)
            .Map(d => d.VariantId, s => s.VariantId)
            .Map(d => d.Quantity, s => s.Quantity)
            .Map(d => d.SalePrice, s => s.Variant.Product.SalePrice)
            .Map(d => d.UnitPrice, s => s.Variant.Product.Price);

        // Create DTO to Model
        TypeAdapterConfig.GlobalSettings.NewConfig<ShoppingCartCreateDto, ShoppingCart>()
            .IgnoreNullValues(true);

        // Update DTO to Model
        TypeAdapterConfig.GlobalSettings.NewConfig<ShoppingCartUpdateDto, ShoppingCart>()
            .IgnoreNullValues(true);
        
        // Model to Cart Summary
        TypeAdapterConfig.GlobalSettings.NewConfig<IEnumerable<ShoppingCartDto>, ShoppingCartSummaryDto>()
            .Map(d => d.ItemCount, s => s.Count())
            .Map(d => d.Subtotal, s => s.Sum(x => x.SalePrice * x.Quantity))
            .Map(d => d.HasUnavailableItems, s => s.Any(x => !x.IsAvailable))
            .Map(d => d.Items, s => s)
            .Map(d => d.TotalAmount, s => s.Sum(x => x.SalePrice * x.Quantity))
            .Map(d => d.TotalQuantity, s => s.Sum(x => x.Quantity))
            .Map(d => d.Subtotal, s => s.Sum(x => x.SalePrice * x.Quantity))
            .Map(d => d.SavingsAmount, s => s.Sum(x => x.UnitPrice * x.Quantity - x.SalePrice * x.Quantity));
        
    }
}