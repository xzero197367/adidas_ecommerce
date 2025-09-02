using Adidas.AdminDashboardMVC.Services;
using Adidas.Application.Contracts.RepositoriesContracts;
using Adidas.Application.Contracts.RepositoriesContracts.Feature;
using Adidas.Application.Contracts.RepositoriesContracts.Main;
using Adidas.Application.Contracts.RepositoriesContracts.Operation;
using Adidas.Application.Contracts.RepositoriesContracts.People;
using Adidas.Application.Contracts.RepositoriesContracts.Separator;
using Adidas.Application.Contracts.RepositoriesContracts.Tracker;
using Adidas.Application.Contracts.ServicesContracts;
using Adidas.Application.Contracts.ServicesContracts.Feature;
using Adidas.Application.Contracts.ServicesContracts.Main;
using Adidas.Application.Contracts.ServicesContracts.Operation;
using Adidas.Application.Contracts.ServicesContracts.People;
using Adidas.Application.Contracts.ServicesContracts.Separator;
using Adidas.Application.Contracts.ServicesContracts.Static;
using Adidas.Application.Contracts.ServicesContracts.Tracker;
using Adidas.Application.Services;
using Adidas.Application.Services.Feature;
using Adidas.Application.Services.Main;
using Adidas.Application.Services.Operation;
using Adidas.Application.Services.People;
using Adidas.Application.Services.Separator;
using Adidas.Application.Services.Static;
using Adidas.Application.Services.Tracker;
using Adidas.Context;
using Adidas.Infra;
using Adidas.Infra.Feature;
using Adidas.Infra.Main;
using Adidas.Infra.Operation;
using Adidas.Infra.People;
using Adidas.Infra.Repositories.Feature;
using Adidas.Infra.Separator;
using Adidas.Infra.Tracker;
using Adidas.Infrastructure.Repositories;
using Microsoft.AspNetCore.Authentication;

public static class MyDependancyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        MyDependancyInjection.ConfigDependancies(services);
        

        return services;
    }

    public static void ConfigDependancies(IServiceCollection services)
    {
        services.AddScoped<AdidasDbContext, AdidasDbContext>();

        // generic scopes registerations
        services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
        services.AddScoped(typeof(IGenericService<,,,>), typeof(GenericService<,,,>));

        AddFeatureServices(services);

        AddMainServices(services);

        AddOperationServices(services);

        AddPeopleServices(services);

        AddSeparatorServices(services);

        AddTrackerServices(services);
    }

    private static void AddFeatureServices(IServiceCollection services)
    {
        // Coupon
        services.AddScoped<ICouponRepository, CouponRepository>();
        services.AddScoped<ICouponService, CouponService>();

        // OrderCoupon
        services.AddScoped<IOrderCouponRepository, OrderCouponRepository>();
        services.AddScoped<IOrderCouponService, OrderCouponService>();

        // ShoppingCart
        services.AddScoped<IShoppingCartRepository, ShoppingCartRepository>();
        services.AddScoped<IShoppingCartService, ShoppingCartService>();

        // Wishlist
        services.AddScoped<IWishlistRepository, WishlistRepository>();
        services.AddScoped<IWishListService, WishListService>();
    }

    private static void AddMainServices(IServiceCollection services)
    {
        // Product
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IProductService, ProductService>();




        // ProductVariant
        services.AddScoped<IProductVariantRepository, ProductVariantRepository>();
        services.AddScoped<IProductVariantService, ProductVariantService>();


        // ProductAttribute
        services.AddScoped<IProductAttributeRepository, ProductAttributeRepository>();
        services.AddScoped<IProductAttributeService, ProductAttributeService>();

        // ProductAttributeValue
        services.AddScoped<IProductAttributeValueRepository, ProductAttributeValueRepository>();
        services.AddScoped<IProductAttributeValueService, ProductAttributeValueService>();

        // ProductImage
        services.AddScoped<IProductImageRepository, ProductImageRepository>();
        services.AddScoped<IProductImageService, ProductImageService>();
    }

    private static void AddOperationServices(IServiceCollection services)
    {
        // Order
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IOrderService, OrderService>();

        // Payment
        services.AddScoped<IPaymentRepository, PaymentRepository>();
        services.AddScoped<IPaymentService, PaymentService>();
        services.AddScoped<IPaymentReportService, PaymentReportService>();

        // Review
        services.AddScoped<IReviewRepository, ReviewRepository>();
        services.AddScoped<IReviewService, ReviewService>();
    }

    private static void AddPeopleServices(IServiceCollection services)
    {
        // Address
        services.AddScoped<IAddressRepository, AddressRepository>();
        services.AddScoped<IAddressService, AddressService>();
        
        // Customer
        services.AddScoped<ICustomerService, CustomerService>();
        
        // User
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IUserProductViewRepository, UserProductViewRepository>();
    }

    private static void AddSeparatorServices(IServiceCollection services)
    {
        // Brand
        services.AddScoped<IBrandRepository, BrandRepository>();
        services.AddScoped<IBrandService, BrandService>();

        // Category
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<ICategoryService, CategoryService>();
    }

    private static void AddTrackerServices(IServiceCollection services)
    {
        // InventoryLog
        services.AddScoped<IInventoryLogRepository, InventoryLogRepository>();
        services.AddScoped<IInventoryService, InventoryService>();
        
        // analytics
        services.AddScoped<IAnalyticsService, AnalyticsService>();
        
        services.AddScoped<IClaimsTransformation, CustomClaimsTransformation>();
        
        // NotificationService
        services.AddScoped<INotificationService, NotificationService>();
    }
}