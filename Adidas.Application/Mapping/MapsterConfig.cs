
using Adidas.Application.Mapping.Feature;
using Adidas.Application.Mapping.Main;
using Adidas.Application.Mapping.Operation;
using Adidas.Application.Mapping.People;
using Adidas.Application.Mapping.Separator;
using Adidas.Application.Mapping.Tracker;

namespace Adidas.Application.Mapping;

public class MapsterConfig
{
    public static void Configure()
    {
        // Feature
        CouponMapConfig.Configure();
        OrderCouponMapConfig.Configure();
        ShoppingCartMapConfig.Configure();
        WishlistMapConfig.Configure();
            
        // Main
        ProductMapConfig.Configure();
        ProductImageMapConfig.Configure();
        ProductAttributeMapConfig.Configure();
        ProductAttributeValueMapConfig.Configure();
        ProductVariantMapConfig.Configure();
        
        // Operation
        OrderMapConfig.Configure();
        PaymentMapConfig.Configure();
        ReviewMapConfig.Configure();
        
        // People
        AddressMapConfig.Configure();
        CustomerMapConfig.Configure();
        UserMapConfig.Configure();
        
        // Separator
        CategoryMapConfig.Configure();
        BrandMapConfig.Configure();
        
        // Tracker
        BrandMapConfig.Configure();
        InventoryLogMapConfig.Configure();
    }
}