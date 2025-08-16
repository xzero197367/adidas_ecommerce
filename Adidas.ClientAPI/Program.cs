// Updated Program.cs with proper using statements and NuGet packages

using Adidas.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Models.People;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Adidas.Application.Contracts.ServicesContracts.People;
using Adidas.Application.Services.People;
using Adidas.Application.Contracts.RepositoriesContracts.People;
using Microsoft.OpenApi.Models;
using Mapster;
using MapsterMapper;
using Microsoft.AspNetCore.Authentication.Google;
using Adidas.Infra.People;

namespace Adidas.ClientAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            // Configure DbContext
            builder.Services.AddDbContext<AdidasDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            // Configure Identity
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

            // Configure JWT Authentication
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

            // Configure Google Authentication
            builder.Services.AddAuthentication()
                .AddGoogle(options =>
                {
                    options.ClientId = builder.Configuration["Authentication:Google:ClientId"];
                    options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];
                });

            // Add Authorization
            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy("CustomerOnly", policy =>
                    policy.RequireClaim(System.Security.Claims.ClaimTypes.Role, "Customer"));

                options.AddPolicy("ActiveUserOnly", policy =>
                    policy.RequireAssertion(context =>
                        context.User.Identity.IsAuthenticated));
            });

            // Register Application Services
            builder.Services.AddScoped<IAddressService, AddressService>();

            // Register Repositories (you'll need to add these)
             builder.Services.AddScoped<IAddressRepository, AddressRepository>();

            // Add Controllers
            builder.Services.AddControllers();

            // Configure Swagger/OpenAPI with JWT support
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
                    Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token in the text input below.",
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

            // Add CORS
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

            // Add Mapster for object mapping (Alternative: use AutoMapper or manual mapping)
            builder.Services.AddMapster();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Adidas Client API v1");
                });
            }

            // Use CORS
            app.UseCors("AllowAngular");

            // Use HTTPS redirection
            app.UseHttpsRedirection();

            // Use Authentication & Authorization
            app.UseAuthentication();
            app.UseAuthorization();

            // Map Controllers
            app.MapControllers();

         

            app.Run();
        }

       

       
    }
}