
using Adidas.Application.Contracts.RepositoriesContracts;
using Adidas.Application.Contracts.RepositoriesContracts.Feature;
using Adidas.Application.Contracts.RepositoriesContracts.Operation;
using Adidas.Application.Contracts.ServicesContracts;
using Adidas.Application.Contracts.ServicesContracts.Feature;
using Adidas.Application.Contracts.ServicesContracts.Main;
using Adidas.Application.Contracts.ServicesContracts.Operation;
using Adidas.Application.Contracts.ServicesContracts.Operation.OrderItemServices;
using Adidas.Application.Services;
using Adidas.Application.Services.Feature;
using Adidas.Application.Services.Main;
using Adidas.Context;
using Adidas.Infra;
using Adidas.Infra.Feature;
using Adidas.Infra.Main;
using Adidas.Infra.Operation;
using Adidas.Infra.Repositories.Feature;

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
    
        
    }
    
    private static void AddOperationServices(IServiceCollection services)
    {
        // OrderItem
        services.AddScoped<IOrderItemRepository, OrderItemRepository>();
        // services.AddScoped<IOrderItemService, OrderItemService>();
        
        // Order
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IOrderService, OrderService>();
    }
    
    private static void AddPeopleServices(IServiceCollection services)
    {
        // Hall
        // services.AddScoped<IHallRepo, HallRepo>();
        // services.AddScoped<IHallService, HallService>();
    }
    
    private static void AddSeparatorServices(IServiceCollection services)
    {
        // Hall
        // services.AddScoped<IHallRepo, HallRepo>();
        // services.AddScoped<IHallService, HallService>();
    }
    
    private static void AddTrackerServices(IServiceCollection services)
    {
        // Hall
        // services.AddScoped<IHallRepo, HallRepo>();
        // services.AddScoped<IHallService, HallService>();
    }

 
}