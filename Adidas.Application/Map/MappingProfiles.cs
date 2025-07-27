

using Adidas.Models.Separator;

namespace Adidas.Application.Map
{
    public class MappingProfiles
    {
//<<<<<<< Updated upstream
//=======
        public MappingProfiles()
        {
            //// Product <=> DTOs
            //CreateMap<Product, ProductDto>()
            //    .ForMember(dest => dest.DisplayPrice, opt => opt.MapFrom(src => src.SalePrice ?? src.Price))
            //    .ForMember(dest => dest.IsOnSale, opt => opt.MapFrom(src => src.SalePrice.HasValue && src.SalePrice < src.Price))
            //    .ForMember(dest => dest.AverageRating, opt => opt.MapFrom(src => src.Reviews.Any() ? src.Reviews.Average(r => r.Rating) : 0))
            //    .ForMember(dest => dest.ReviewCount, opt => opt.MapFrom(src => src.Reviews.Count))
            //    .ForMember(dest => dest.InStock, opt => opt.MapFrom(src => src.Variants.Any(v => v.StockQuantity > 0)));
            //CreateMap<CreateProductDto, Product>()
            //    .ForMember(dest => dest.Sku, opt => opt.Ignore()) // SKU is generated in ProductService
            //    .ForMember(dest => dest.Variants, opt => opt.MapFrom(src => src.Variants))
            //    .ForMember(dest => dest.Images, opt => opt.MapFrom(src => src.Images));
            //CreateMap<UpdateProductDto, Product>()
            //    .ForMember(dest => dest.Sku, opt => opt.Ignore()); // SKU is not updated



            //// ProductVariant <=> DTOs
            //CreateMap<ProductVariant, ProductVariantDto>()
            //    .ForMember(dest => dest.ColorHex, opt => opt.MapFrom(src => src.ImageUrl)); // Assuming ImageUrl might store ColorHex
            //CreateMap<CreateProductVariantDto, ProductVariant>()
            //    .ForMember(dest => dest.Sku, opt => opt.Ignore()); // SKU may be generated
            //CreateMap<UpdateProductVariantDto, ProductVariant>()
            //    .ForMember(dest => dest.Sku, opt => opt.Ignore());



            // Category <=> DTOs
            CreateMap<CreateCategoryDto, Category>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.IsActive, opt => opt.Ignore()) // Will be set in BeforeCreate
                .ForMember(dest => dest.AddedById, opt => opt.Ignore())
                .ForMember(dest => dest.AddedBy, opt => opt.Ignore())
                .ForMember(dest => dest.ParentCategory, opt => opt.Ignore())
                .ForMember(dest => dest.SubCategories, opt => opt.Ignore())
                .ForMember(dest => dest.Products, opt => opt.Ignore());

            CreateMap<UpdateCategoryDto, Category>()
                .ForMember(dest => dest.Id, opt => opt.Ignore()) // ID should not be updated
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore()) // Will be set in BeforeUpdate
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.IsActive, opt => opt.Ignore())
                .ForMember(dest => dest.AddedById, opt => opt.Ignore())
                .ForMember(dest => dest.AddedBy, opt => opt.Ignore())
                .ForMember(dest => dest.ParentCategory, opt => opt.Ignore())
                .ForMember(dest => dest.SubCategories, opt => opt.Ignore())
                .ForMember(dest => dest.Products, opt => opt.Ignore());

            CreateMap<Category, CategoryResponseDto>()
                .ForMember(dest => dest.ParentCategoryName, opt => opt.MapFrom(src => src.ParentCategory != null ? src.ParentCategory.Name : null))
                .ForMember(dest => dest.ProductCount, opt => opt.MapFrom(src => src.Products != null ? src.Products.Count : 0))
                .ForMember(dest => dest.SubCategoryCount, opt => opt.MapFrom(src => src.SubCategories != null ? src.SubCategories.Count : 0))
                .ForMember(dest => dest.SubCategories, opt => opt.Ignore()); // Will be set manually in service

            CreateMap<Category, CategoryListDto>()
                .ForMember(dest => dest.ParentCategoryName, opt => opt.MapFrom(src => src.ParentCategory != null ? src.ParentCategory.Name : null))
                .ForMember(dest => dest.ProductCount, opt => opt.MapFrom(src => src.Products != null ? src.Products.Count : 0))
                .ForMember(dest => dest.SubCategoryCount, opt => opt.MapFrom(src => src.SubCategories != null ? src.SubCategories.Count : 0));

            CreateMap<Category, CategoryHierarchyDto>()
                .ForMember(dest => dest.Level, opt => opt.Ignore()); // Will be set manually in service

            // Additional mapping for UpdateCategoryDto to include Id
            CreateMap<Category, UpdateCategoryDto>();
        }

        //// User <=> DTOs
        //CreateMap<User, UserDto>()
        //    .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => $"{src.FirstName} {src.LastName}"))
        //    .ForMember(dest => dest.DefaultAddress, opt => opt.MapFrom(src => src.Addresses.FirstOrDefault(a => a.IsDefault)));
        //CreateMap<CreateUserDto, User>()
        //    .ForMember(dest => dest.PasswordHash, opt => opt.Ignore()); // Handled in UserService
        //CreateMap<UpdateUserDto, User>();
        //CreateMap<RegisterUserDto, User>()
        //    .ForMember(dest => dest.PasswordHash, opt => opt.Ignore()) // Handled in UserService
        //    .ForMember(dest => dest.Role, opt => opt.MapFrom(src => UserRole.Customer));



        //// Address <=> DTOs
        //CreateMap<Address, AddressDto>()
        //    .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => $"{src.AddedBy.FirstName} {src.AddedBy.LastName}"))
        //    .ForMember(dest => dest.FormattedAddress, opt => opt.MapFrom(src => $"{src.StreetAddress}, {src.City}, {src.StateProvince} {src.PostalCode}, {src.Country}"));
        //CreateMap<CreateAddressDto, Address>();
        //CreateMap<UpdateAddressDto, Address>();



        //// Order <=> DTOs
        //CreateMap<Order, OrderDto>()
        //    .ForMember(dest => dest.ItemCount, opt => opt.MapFrom(src => src.OrderItems.Sum(oi => oi.Quantity)))
        //    .ForMember(dest => dest.CanBeCancelled, opt => opt.MapFrom(src => src.OrderStatus == OrderStatus.Pending));
        //CreateMap<CreateOrderDto, Order>()
        //    .ForMember(dest => dest.OrderNumber, opt => opt.Ignore()) // Generated in OrderService
        //    .ForMember(dest => dest.OrderDate, opt => opt.MapFrom(src => DateTime.UtcNow))
        //    .ForMember(dest => dest.OrderStatus, opt => opt.MapFrom(src => OrderStatus.Pending));
        //CreateMap<UpdateOrderDto, Order>();
        //CreateMap<CreateOrderFromCartDto, Order>()
        //    .ForMember(dest => dest.OrderNumber, opt => opt.Ignore())
        //    .ForMember(dest => dest.OrderDate, opt => opt.MapFrom(src => DateTime.UtcNow))
        //    .ForMember(dest => dest.OrderStatus, opt => opt.MapFrom(src => OrderStatus.Pending));

        //// OrderItem <=> DTOs
        //CreateMap<OrderItem, OrderItemDto>();



        //// ShoppingCart <=> DTOs
        //CreateMap<ShoppingCart, ShoppingCartItemDto>()
        //    .ForMember(dest => dest.UnitPrice, opt => opt.MapFrom(src => (src.Variant.Product.SalePrice ?? src.Variant.Product.Price) + src.Variant.PriceAdjustment))
        //    .ForMember(dest => dest.TotalPrice, opt => opt.MapFrom(src => ((src.Variant.Product.SalePrice ?? src.Variant.Product.Price) + src.Variant.PriceAdjustment) * src.Quantity));
        //CreateMap<AddToCartDto, ShoppingCart>()
        //    .ForMember(dest => dest.AddedAt, opt => opt.MapFrom(src => DateTime.UtcNow));
        //CreateMap<UpdateCartItemDto, ShoppingCart>();



        //// Review <=> DTOs
        //CreateMap<Review, ReviewDto>()
        //    .ForMember(dest => dest.IsPending, opt => opt.MapFrom(src => !src.IsApproved && string.IsNullOrEmpty(src.AddedById))) // Assuming AddedById as RejectionReason
        //    .ForMember(dest => dest.IsRejected, opt => opt.MapFrom(src => !src.IsApproved && !string.IsNullOrEmpty(src.AddedById)));
        //CreateMap<CreateReviewDto, Review>()
        //    .ForMember(dest => dest.IsApproved, opt => opt.MapFrom(src => false))
        //    .ForMember(dest => dest.IsVerifiedPurchase, opt => opt.MapFrom(src => false)); // Needs verification logic
        //CreateMap<UpdateReviewDto, Review>();


        //// Discount <=> DTOs
        //CreateMap<Discount, DiscountDto>()
        //    .ForMember(dest => dest.IsValid, opt => opt.MapFrom(src => DateTime.UtcNow >= src.ValidFrom && DateTime.UtcNow <= src.ValidTo))
        //    .ForMember(dest => dest.CanBeUsed, opt => opt.MapFrom(src => src.UsageLimit == 0 || src.UsedCount < src.UsageLimit))
        //    .ForMember(dest => dest.RemainingUses, opt => opt.MapFrom(src => src.UsageLimit == 0 ? int.MaxValue : Math.Max(0, src.UsageLimit - src.UsedCount)));
        //CreateMap<CreateDiscountDto, Discount>();
        //CreateMap<UpdateDiscountDto, Discount>();


        //// OrderCoupon <=> DTOs
        //CreateMap<OrderCoupon, OrderCouponDto>();


        // Brand <=> DTOs
        // Brand mappings
        CreateMap<CreateBrandDto, Brand>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.IsActive, opt => opt.Ignore()) // Will be set in BeforeCreate
                .ForMember(dest => dest.AddedById, opt => opt.Ignore())
                .ForMember(dest => dest.AddedBy, opt => opt.Ignore())
                .ForMember(dest => dest.Products, opt => opt.Ignore());

            CreateMap<UpdateBrandDto, Brand>()
                .ForMember(dest => dest.Id, opt => opt.Ignore()) // ID should not be updated
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore()) // Will be set in BeforeUpdate
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.IsActive, opt => opt.Ignore())
                .ForMember(dest => dest.AddedById, opt => opt.Ignore())
                .ForMember(dest => dest.AddedBy, opt => opt.Ignore())
                .ForMember(dest => dest.Products, opt => opt.Ignore());

            CreateMap<Brand, BrandResponseDto>()
                .ForMember(dest => dest.ProductCount, opt => opt.MapFrom(src => src.Products != null ? src.Products.Count : 0));

            CreateMap<Brand, BrandListDto>()
                .ForMember(dest => dest.ProductCount, opt => opt.MapFrom(src => src.Products != null ? src.Products.Count : 0));

            // Additional mapping for UpdateBrandDto to include Id
            CreateMap<Brand, UpdateBrandDto>();

            //// ProductImage <=> DTOs
            //CreateMap<ProductImage, ProductImageDto>();
            //CreateMap<CreateProductImageDto, ProductImage>();
            //CreateMap<UpdateProductImageDto, ProductImage>();

            //// Wishlist <=> DTOs
            //CreateMap<Wishlist, WishlistItemDto>();
            //CreateMap<AddToWishlistDto, Wishlist>()
            //    .ForMember(dest => dest.AddedAt, opt => opt.MapFrom(src => DateTime.UtcNow));

            //// ProductAttribute <=> DTOs
            //CreateMap<ProductAttribute, ProductAttributeDto>();
            //CreateMap<ProductAttributeValue, ProductAttributeValueDto>();

            //// InventoryLog <=> DTOs
            //CreateMap<InventoryLog, InventoryLogDto>();

            //// Payment <=> DTOs
            //CreateMap<Payment, PaymentDto>();

            //// Analytics & Reporting DTOs (No direct entity mappings)
            //CreateMap<Order, OrderSummaryDto>()
            //    .ForMember(dest => dest.TotalSales, opt => opt.Ignore()) // Requires aggregation
            //    .ForMember(dest => dest.TotalOrders, opt => opt.Ignore())
            //    .ForMember(dest => dest.AverageOrderValue, opt => opt.Ignore())
            //    .ForMember(dest => dest.OrdersByStatus, opt => opt.Ignore());
            //CreateMap<ProductVariant, LowStockAlertDto>()
            //    .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product.Name))
            //    .ForMember(dest => dest.VariantDetails, opt => opt.MapFrom(src => $"{src.Color} - {src.Size}"))
            //    .ForMember(dest => dest.CurrentStock, opt => opt.MapFrom(src => src.StockQuantity))
            //    .ForMember(dest => dest.ReorderLevel, opt => opt.Ignore()); // Requires configuration
            //CreateMap<Product, ProductStockDto>()
            //    .ForMember(dest => dest.TotalStock, opt => opt.MapFrom(src => src.Variants.Sum(v => v.StockQuantity)))
            //    .ForMember(dest => dest.VariantCount, opt => opt.MapFrom(src => src.Variants.Count))
            //    .ForMember(dest => dest.InventoryValue, opt => opt.MapFrom(src => src.Variants.Sum(v => v.StockQuantity * (v.Product.SalePrice ?? v.Product.Price + v.PriceAdjustment))));
            //CreateMap<Order, DailySalesDto>()
            //    .ForMember(dest => dest.Date, opt => opt.MapFrom(src => src.OrderDate.Date))
            //    .ForMember(dest => dest.Sales, opt => opt.MapFrom(src => src.TotalAmount))
            //    .ForMember(dest => dest.Orders, opt => opt.Ignore()); // Requires aggregation
            //CreateMap<Product, CategorySalesDto>()
            //    .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category.Name))
            //    .ForMember(dest => dest.Sales, opt => opt.Ignore()) // Requires order data
            //    .ForMember(dest => dest.ProductsSold, opt => opt.Ignore());
            //CreateMap<Product, PopularProductDto>()
            //    .ForMember(dest => dest.UnitsSold, opt => opt.Ignore()) // Requires order data
            //    .ForMember(dest => dest.Revenue, opt => opt.Ignore());
            //CreateMap<Category, CategoryPerformanceDto>()
            //    .ForMember(dest => dest.ProductCount, opt => opt.MapFrom(src => src.Products.Count))
            //    .ForMember(dest => dest.TotalSales, opt => opt.Ignore()) // Requires order data
            //    .ForMember(dest => dest.OrderCount, opt => opt.Ignore());
            //CreateMap<User, CustomerSegmentDto>()
            //    .ForMember(dest => dest.SegmentName, opt => opt.Ignore()) // Requires segmentation logic
            //    .ForMember(dest => dest.CustomerCount, opt => opt.Ignore())
            //    .ForMember(dest => dest.AverageOrderValue, opt => opt.Ignore());
        }
//>>>>>>> Stashed changes
    }
}
