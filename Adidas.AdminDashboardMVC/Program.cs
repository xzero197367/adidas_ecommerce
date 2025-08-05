
using Adidas.Application.Contracts.ServicesContracts.Feature;
using Adidas.Application.Map.Feature; // Make sure this points to your profile
using Adidas.Application.Services.Feature;

using Adidas.Application.Contracts.RepositoriesContracts.Main;
using Adidas.Application.Contracts.RepositoriesContracts.Operation;
using Adidas.Application.Contracts.RepositoriesContracts.People;
using Adidas.Application.Contracts.RepositoriesContracts.Separator;
using Adidas.Application.Contracts.ServicesContracts.People;
using Adidas.Application.Contracts.ServicesContracts.Static;
using Adidas.Application.Map;

using Adidas.Application.Services.People;
using Adidas.Application.Services.Static;
using Adidas.Context;

using Adidas.Infra.Main;
using Adidas.Infra.Operation;
using Adidas.Infra.Separator;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Models.People;



var builder = WebApplication.CreateBuilder(args);

// 1. Configure EF Core
builder.Services.AddDbContext<AdidasDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 2. Configure Identity
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

// 3. Configure Authorization
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
    options.AddPolicy("EmployeeOrAdmin", policy => policy.RequireRole("Admin", "Employee"));
});

// 4. Register MVC
builder.Services.AddControllersWithViews();


// 5. Register your services


// ✅ 6. Register AutoMapper - THIS IS THE CORRECT LINE
builder.Services.AddAutoMapper(typeof(CouponMappingProfile).Assembly);
builder.Services.AddAutoMapper(typeof(MappingProfiles).Assembly);
// 5. NOW add your custom services (after Identity is configured)
builder.Services.AddScoped<ICustomerService, CustomerService>(); 
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IProductVariantRepository, ProductVariantRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<IAnalyticsService, AnalyticsService>();
builder.Services.AddScoped<ICouponService, CouponService>();
builder.Services.AddScoped<ICouponRepository, CouponRepository>();



// 7. Build the app
var app = builder.Build();

// 8. Middleware pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}

app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Dashboard}/{action=Index}/{id?}");

app.Run();
