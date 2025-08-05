using Adidas.Application.Map.Feature;
using Adidas.Application.Map.Operation;
using Adidas.DTOs.Feature.CouponDTOs;
using Adidas.DTOs.Feature.OrderCouponDTOs;
using Adidas.DTOs.Feature.ShoppingCartDTOS;
using Adidas.DTOs.Feature.WishLIstDTOS;
using Adidas.DTOs.Main.Product_DTOs;
using Adidas.DTOs.Main.Product_Variant_DTOs;
using Adidas.DTOs.Main.ProductAttributeDTOs;
using Adidas.DTOs.Main.ProductAttributeValueDTOs;
using Adidas.DTOs.Main.ProductImageDTOs;
using Adidas.DTOs.Operation.OrderDTOs.Create;
using Adidas.DTOs.Operation.OrderDTOs.Query;
using Adidas.DTOs.Operation.OrderDTOs.Result;
using Adidas.DTOs.Operation.OrderDTOs.Update;
using Adidas.DTOs.Operation.PaymentDTOs.Create;
using Adidas.DTOs.Operation.PaymentDTOs.Query;
using Adidas.DTOs.Operation.PaymentDTOs.Result;
using Adidas.DTOs.Operation.PaymentDTOs.Statistics;
using Adidas.DTOs.Operation.PaymentDTOs.Update;
using Adidas.DTOs.Operation.ReviewDTOs.Create;
using Adidas.DTOs.Operation.ReviewDTOs.Query;
using Adidas.DTOs.Operation.ReviewDTOs.Result;
using Adidas.DTOs.Operation.ReviewDTOs.Shared;
using Adidas.DTOs.Operation.ReviewDTOs.Update;
using Adidas.DTOs.People.Address_DTOs;
using Adidas.DTOs.People.Customer_DTOs;
using Adidas.DTOs.Separator.Brand_DTOs;
using Adidas.DTOs.Separator.Category_DTOs;
using Adidas.DTOs.Static;
using Adidas.DTOs.Tracker;
using Adidas.Models.Feature;
using Adidas.Models.Main;
using Adidas.Models.Operation;
using Adidas.Models.Separator;
using Adidas.Models.Tracker;
using AutoMapper;
using Models.Feature;
using Models.People;

namespace Adidas.Application.Map
{
    public class MappingProfiles : Profile
    {
        public MappingProfiles()
        {
            // Product <=> DTOs
            CreateMap<Product, ProductDto>()
                .ForMember(dest => dest.DisplayPrice, opt => opt.MapFrom(src => src.SalePrice ?? src.Price))
                .ForMember(dest => dest.IsOnSale, opt => opt.MapFrom(src => src.SalePrice.HasValue && src.SalePrice < src.Price))
                .ForMember(dest => dest.AverageRating, opt => opt.MapFrom(src => src.Reviews.Any() ? src.Reviews.Average(r => r.Rating) : 0))
                .ForMember(dest => dest.ReviewCount, opt => opt.MapFrom(src => src.Reviews.Count))
                .ForMember(dest => dest.InStock, opt => opt.MapFrom(src => src.Variants.Any(v => v.StockQuantity > 0)));
            CreateMap<CreateProductDto, Product>()
                .ForMember(dest => dest.Sku, opt => opt.Ignore())
                .ForMember(dest => dest.Variants, opt => opt.MapFrom(src => src.Variants))
                .ForMember(dest => dest.Images, opt => opt.MapFrom(src => src.Images));
            CreateMap<UpdateProductDto, Product>()
                .ForMember(dest => dest.Sku, opt => opt.Ignore());

            // ProductVariant <=> DTOs
            CreateMap<ProductVariant, ProductVariantDto>()
                .ForMember(dest => dest.ColorHex, opt => opt.MapFrom(src => src.ImageUrl));
            CreateMap<CreateProductVariantDto, ProductVariant>()
                .ForMember(dest => dest.Sku, opt => opt.Ignore());
            CreateMap<UpdateProductVariantDto, ProductVariant>()
                .ForMember(dest => dest.Sku, opt => opt.Ignore());


            // Category <=> DTOs
            CreateMap<Category, CategoryDto>()
                .ForMember(dest => dest.HasSubCategories, opt => opt.MapFrom(src => src.SubCategories.Any()))
                .ForMember(dest => dest.ProductCount, opt => opt.MapFrom(src => src.Products.Count));
            CreateMap<CreateCategoryDto, Category>().ReverseMap();
            CreateMap<UpdateCategoryDto, Category>().ReverseMap();
            CreateMap<Category, CategoryResponseDto>()
                .ForMember(dest => dest.ParentCategoryName, opt => opt.MapFrom(src => src.ParentCategory != null ? src.ParentCategory.Name : null))
                .ForMember(dest => dest.ProductCount, opt => opt.MapFrom(src => src.Products.Count))
                .ForMember(dest => dest.SubCategoryCount, opt => opt.MapFrom(src => src.SubCategories.Count))
                .ForMember(dest => dest.SubCategories, opt => opt.Ignore());
            CreateMap<Category, CategoryHierarchyDto>()
                .ForMember(dest => dest.Level, opt => opt.Ignore());
            CreateMap<Category, CategoryListDto>();
            CreateMap<CategoryListDto, CategoryDto>();

            // Brand <=> DTOs
            CreateMap<Brand, BrandDto>().ReverseMap();
            CreateMap<Brand, BrandResponseDto>().ReverseMap();
            CreateMap<Brand, BrandListDto>().ReverseMap();
            CreateMap<CreateBrandDto, Brand>().ReverseMap();
            CreateMap<UpdateBrandDto, Brand>().ReverseMap();

            // Address <=> DTOs
            CreateMap<Address, AddressDto>()
                .ForMember(dest => dest.FullAddress, opt => opt.MapFrom(src => $"{src.StreetAddress}, {src.City}, {src.StateProvince} {src.PostalCode}, {src.Country}"));
            CreateMap<CreateAddressDto, Address>().ReverseMap();
            CreateMap<UpdateAddressDto, Address>().ReverseMap();

            // Order <=> DTOs
            CreateMap<Order, OrderDto>().ReverseMap();
            CreateMap<CreateOrderDto, Order>().ReverseMap();
            CreateMap<CreateOrderFromCartDto, Order>().ReverseMap();
            CreateMap<UpdateOrderDto, Order>().ReverseMap();
            CreateMap<UpdateOrderStatusDto, Order>().ReverseMap();
            CreateMap<Order, OrderSummaryDto>().ReverseMap();
            CreateMap<Order, OrderQueryDto>().ReverseMap();
            CreateMap<Order, PagedOrderResultDto>().ReverseMap();

            // OrderItem <=> DTOs
            CreateMap<OrderItem, OrderItemDto>().ReverseMap();
            CreateMap<CreateOrderItemDto, OrderItem>().ReverseMap();

            // Payment <=> DTOs
            CreateMap<Payment, PaymentDto>().ReverseMap();
            CreateMap<CreatePaymentDto, Payment>().ReverseMap();
            CreateMap<UpdatePaymentDto, Payment>().ReverseMap();
            CreateMap<Payment, PaymentWithOrderDto>().ReverseMap();
            CreateMap<Payment, PaymentFilterDto>().ReverseMap();
            CreateMap<Payment, PagedPaymentDto>().ReverseMap();
            CreateMap<Payment, PaymentStatsDto>().ReverseMap();


            // Review <=> DTOs
            CreateMap<Review, ReviewDto>().ReverseMap();
            CreateMap<CreateReviewDto, Review>().ReverseMap();
            CreateMap<UpdateReviewDto, Review>().ReverseMap();
            CreateMap<Review, ReviewWithDetailsDto>().ReverseMap();
            CreateMap<Review, ReviewModerationDto>().ReverseMap();
            CreateMap<Review, AdminUpdateReviewDto>().ReverseMap();
            CreateMap<Review, PagedReviewDto>().ReverseMap();
            CreateMap<Review, ProductReviewSummaryDto>().ReverseMap();
            CreateMap<Review, ReviewStatsDto>().ReverseMap();
            CreateMap<Review, UserReviewSummaryDto>().ReverseMap();

            // Customer <=> DTOs
            CreateMap<User, CustomerDto>()
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.UserName))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.JoinDate, opt => opt.MapFrom(src => src.CreatedAt ))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.IsActive ? "Active" : "Inactive"));
            CreateMap<User, CustomerDetailsDto>().ReverseMap();
            CreateMap<UpdateCustomerDto, User>().ReverseMap();

            // Coupon <=> DTOs
            CreateMap<Coupon, CouponUpdateDto>().ReverseMap();
            CreateMap<OrderCoupon, OrderCouponDto>().ReverseMap();
            CreateMap<OrderCoupon, OrderCouponUpdateDto>().ReverseMap();
            CreateMap<OrderCoupon, OrderCouponCreateDto>().ReverseMap();

            // ShoppingCart <=> DTOs
            CreateMap<ShoppingCart, ShoppingCartItemDto>().ReverseMap();
            CreateMap<ShoppingCart, AddToCartDto>().ReverseMap();
            CreateMap<ShoppingCart, UpdateCartItemDto>().ReverseMap();

            // Wishlist <=> DTOs
            CreateMap<Wishlist, WishlistItemDto>().ReverseMap();
            CreateMap<AddToWishlistDto, Wishlist>().ReverseMap();

            // ProductAttribute <=> DTOs
            CreateMap<ProductAttribute, ProductAttributeDto>().ReverseMap();
            CreateMap<ProductAttribute, CreateProductAttributeDto>().ReverseMap();
            CreateMap<ProductAttribute, UpdateProductAttributeDto>().ReverseMap();

            // ProductAttributeValue <=> DTOs
            CreateMap<ProductAttributeValue, ProductAttributeValueDto>().ReverseMap();
            CreateMap<ProductAttributeValue, CreateProductAttributeValueDto>().ReverseMap();
            CreateMap<ProductAttributeValue, UpdateProductAttributeValueDto>().ReverseMap();

            // ProductImage <=> DTOs
            CreateMap<ProductImage, ProductImageDto>().ReverseMap();
            CreateMap<CreateProductImageDto, ProductImage>().ReverseMap();
            CreateMap<UpdateProductImageDto, ProductImage>().ReverseMap();

            // Analytics / Reporting
            CreateMap<Product, ProductSummaryDto>().ReverseMap();
            CreateMap<User, UserSummaryDto>().ReverseMap();
        }
    }
}