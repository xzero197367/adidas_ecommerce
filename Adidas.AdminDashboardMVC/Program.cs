using Adidas.AdminDashboardMVC.Helpers;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
//using AutoMapper.Extensions.Microsoft.DependencyInjection;
using Adidas.Context;
using Adidas.AdminDashboardMVC.Middleware;
using Adidas.AdminDashboardMVC.Services;
using Models.People;

// Services
using Adidas.Application.Services.Feature;
using Adidas.Application.Services.People;
using Adidas.Application.Services.Static;

// Repositories
using Adidas.Infra.Main;
using Adidas.Infra.Operation;
using Adidas.Infra.Separator;

// Contracts
using Adidas.Application.Contracts.RepositoriesContracts.Main;
using Adidas.Application.Contracts.RepositoriesContracts.Operation;
using Adidas.Application.Contracts.RepositoriesContracts.People;
using Adidas.Application.Contracts.RepositoriesContracts.Separator;
using Adidas.Application.Contracts.ServicesContracts.Feature;
using Adidas.Application.Contracts.ServicesContracts.People;
using Adidas.Application.Contracts.ServicesContracts.Static;
using Microsoft.AspNetCore.Authentication;
using Adidas.Application.Contracts.ServicesContracts.Operation;
using Adidas.Application.Services.Operation;
using Adidas.Infrastructure.Repositories;
using Adidas.Application.Contracts.ServicesContracts.Separator;
using Adidas.Application.Mapping;
using Adidas.Application.Services.Separator;
using Microsoft.AspNetCore.Mvc.Razor;
using Adidas.Application.Contracts.ServicesContracts.Main;
using Adidas.Application.Services.Main;
using CloudinaryDotNet;
using Microsoft.Extensions.Options;
using Adidas.Infra.Settings;

var builder = WebApplication.CreateBuilder(args);

#region 1. EF Core

builder.Services.AddDbContext<AdidasDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

#endregion

#region CloudinaryDotNet
builder.Services.Configure<CloudinarySettings>(
    builder.Configuration.GetSection("CloudinarySettings"));

builder.Services.AddSingleton<Cloudinary>(sp =>
{
    var settings = sp.GetRequiredService<IOptions<CloudinarySettings>>().Value;
    var account = new Account(settings.CloudName, settings.ApiKey, settings.ApiSecret);
    return new Cloudinary(account);
});
#endregion
#region 2. Identity Configuration

builder.Services.AddIdentity<User, IdentityRole>(options =>
    {
        options.Password.RequireDigit = true;
        options.Password.RequiredLength = 6;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequireUppercase = true;
        options.Password.RequireLowercase = false;

        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.AllowedForNewUsers = true;

        options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
        options.User.RequireUniqueEmail = true;

        options.SignIn.RequireConfirmedEmail = false;
        options.SignIn.RequireConfirmedPhoneNumber = false;
    })
    .AddEntityFrameworkStores<AdidasDbContext>()
    .AddDefaultTokenProviders();


// 3. Add Authorization

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromHours(8);
    options.SlidingExpiration = true;
});

#endregion

#region 3. Authentication - Google

builder.Services.AddAuthentication()
    .AddGoogle(options =>
    {
        options.ClientId = builder.Configuration["Authentication:Google:ClientId"];
        options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];
        Console.WriteLine("Redirect URI: " + options.CallbackPath);
    });

#endregion

#region 4. Authorization Policies

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
    options.AddPolicy("EmployeeOrAdmin", policy => policy.RequireRole("Admin", "Employee"));

    options.AddPolicy("ActiveUser", policy => policy.RequireClaim("IsActive", "True"));
});

#endregion

#region 5. MVC

builder.Services.AddControllersWithViews();


// 5. NOW add your custom services (after Identity is configured)
// builder.Services.AddScoped<ICustomerService, CustomerService>();
// builder.Services.AddScoped<IReviewRepository, ReviewRepository>();
 builder.Services.AddScoped<IOrderRepository, OrderRepository>();
 builder.Services.AddScoped<IOrderItemRepository, OrderItemRepository>();
//
// builder.Services.AddScoped<IReviewService, ReviewService>();
builder.Services.AddScoped<IOrderService, OrderService>();
//builder.Services.AddAutoMapper(Program);

#endregion

#region 6. Mapster Configuration (v12+)

MapsterConfig.Configure();

#endregion

#region 7. Application Services & Repositories
builder.Services.AddScoped<IUserProductViewRepository, UserProductViewRepository>();

// Register the service that depends on the repository
builder.Services.AddScoped<IProductService, ProductService>();
// viewlocation exapnder
// add custom view locations
builder.Services.Configure<RazorViewEngineOptions>(options =>
{
    options.ViewLocationExpanders.Add(new ViewLocationExpander());
});

MyDependancyInjection.ConfigDependancies(builder.Services);

#endregion


var app = builder.Build();

#region 8. Middleware Pipeline

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");


    app.UseStatusCodePagesWithReExecute("/Account/Error/{0}");
}

app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseActiveUserMiddleware(); // Custom middleware

#endregion

#region 9. Routing

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Dashboard}/{action=Index}/{id?}");


#endregion

#region 10. Seed Roles & Admin

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        await DatabaseSeeder.SeedRolesAndAdminAsync(services);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database.");
    }
}

#endregion

app.Run();