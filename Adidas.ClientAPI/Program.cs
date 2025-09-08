// Program.cs 

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
using Adidas.Infra.Feature;
using Adidas.Infra.Main;
using Adidas.Infra.Operation;
using Adidas.Infra.People;
using Adidas.Infra.Repositories.Feature;
using Adidas.Infra.Separator;
using Adidas.Infra.Settings;
using Adidas.Infra.Tracker;
using Adidas.Infrastructure.Repositories;
using CloudinaryDotNet;
using Mapster;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Models.People;
using System.Text;


namespace Adidas.ClientAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            #region DbContext

            builder.Services.AddDbContext<AdidasDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            #endregion

            #region Identity

            builder.Services.AddIdentity<User, IdentityRole>(options =>
                {
                    // Password settings
                    options.Password.RequireDigit = true;
                    options.Password.RequireLowercase = true;
                    options.Password.RequireUppercase = true;
                    options.Password.RequireNonAlphanumeric = false;
                    options.Password.RequiredLength = 6;

                    // User settings
                    options.User.RequireUniqueEmail = true;

                    // Lockout settings
                    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(30);
                    options.Lockout.MaxFailedAccessAttempts = 5;

                    // Sign-in settings
                    options.SignIn.RequireConfirmedEmail = false; // Set to true in production
                })
                .AddEntityFrameworkStores<AdidasDbContext>()
                .AddDefaultTokenProviders();

            #endregion

            #region Authentication - JWT

            var jwtSettings = builder.Configuration.GetSection("JwtSettings");
            var secretKey = jwtSettings["SecretKey"];

            builder.Services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = jwtSettings["Issuer"],
                        ValidAudience = jwtSettings["Audience"],
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(secretKey)),
                        ClockSkew = TimeSpan.Zero
                    };
                });

            #endregion

            #region Authentication - Google

            builder.Services.AddAuthentication()
                .AddGoogle(options =>
                {
                    options.ClientId = builder.Configuration["Authentication:Google:ClientId"];
                    options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];
                });

            #endregion

            #region Authorization

            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy("CustomerOnly", policy =>
                    policy.RequireClaim(System.Security.Claims.ClaimTypes.Role, "Customer"));

                options.AddPolicy("ActiveUserOnly", policy =>
                    policy.RequireAssertion(context =>
                        context.User.Identity.IsAuthenticated));
            });

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

            #region Application Services & Repositories

            builder.Services.AddScoped<IAuthService, AuthService>();
            builder.Services.AddScoped<IReviewRepository, ReviewRepository>();
            builder.Services.AddScoped<IReviewService, ReviewService>();
            builder.Services.AddScoped<IUserProductViewRepository, UserProductViewRepository>();



            builder.Services.AddScoped<IAddressService, AddressService>();
            builder.Services.AddScoped<ProductReviewService, ProductReviewService>();
            // Register Services
            builder.Services.AddScoped<IRecommendationService, RecommendationService>();
            builder.Services.AddScoped<IProductService, ProductService>();
            builder.Services.AddScoped<ICategoryService, CategoryService>();
            builder.Services.AddScoped<ICouponService, CouponService>();
            builder.Services.AddScoped<IPaymentService, PaymentService>();
            builder.Services.AddScoped<IProductVariantService, ProductVariantService>();
            builder.Services.AddScoped<IProductImageService, ProductImageService>();
            builder.Services.AddScoped<IOrderService, OrderService>();
            builder.Services.AddScoped<IInventoryService, InventoryService>();
            builder.Services.AddScoped<INotificationService, NotificationService>();
            // Register Repositories
            builder.Services.AddScoped<IAddressRepository, AddressRepository>();
            builder.Services.AddScoped<IProductRepository, ProductRepository>();
            builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
            builder.Services.AddScoped<IProductVariantRepository, ProductVariantRepository>();
            builder.Services.AddScoped<ICouponRepository, CouponRepository>();
            builder.Services.AddScoped<IOrderRepository, OrderRepository>();
            builder.Services.AddScoped<IOrderItemRepository, OrderItemRepository>();
            builder.Services.AddScoped<IOrderCouponRepository, OrderCouponRepository>();
            builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();
            builder.Services.AddScoped<IProductImageRepository, ProductImageRepository>();
            builder.Services.AddScoped<IWishListService, WishListService>();
            builder.Services.AddScoped<IWishlistRepository, WishlistRepository>();
            builder.Services.AddScoped<IInventoryLogRepository, InventoryLogRepository>();
            builder.Services.AddScoped<IShoppingCartService, ShoppingCartService>();
            builder.Services.AddScoped<IShoppingCartRepository, ShoppingCartRepository>();
            builder.Services.AddScoped<IUserRepository, UserRepository>();
            // Register PayPal Services with HttpClient
            builder.Services.AddHttpClient<IPayPalService, PayPalRestService>(client =>
            {
                // Keep BaseAddress configurable if needed
                // client.BaseAddress = new Uri("https://api-m.sandbox.paypal.com");
            });


            #region Controllers

            builder.Services.AddControllers();

            #endregion

            #region Swagger

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Adidas Client API",
                    Version = "v1",
                    Description = "API for Adidas E-commerce Client Application"
                });

                // Add JWT Authentication to Swagger
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description =
                        "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token in the text input below.",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        new string[] { }
                    }
                });
            });

            #endregion

            #region CORS

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAngular", policy =>
                {
                    policy.WithOrigins("http://localhost:4200") // Angular default port
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .AllowCredentials();
                });
            });

            #endregion

            #region Mapster

            builder.Services.AddMapster();

            #endregion

            var app = builder.Build();

            #region Middleware

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(c => { c.SwaggerEndpoint("/swagger/v1/swagger.json", "Adidas Client API v1"); });
            }

            app.UseCors("AllowAngular");
            app.UseHttpsRedirection();
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllers();

            #endregion

            // Ensure roles exist
            using (var scope = app.Services.CreateScope())
            {
                var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

                string[] roleNames = { "Admin", "Customer" };

                foreach (var roleName in roleNames)
                {
                    if (!roleManager.RoleExistsAsync(roleName).GetAwaiter().GetResult())
                    {
                        roleManager.CreateAsync(new IdentityRole(roleName)).GetAwaiter().GetResult();
                    }
                }
            }


            app.Run();
        }
    }
}

#endregion