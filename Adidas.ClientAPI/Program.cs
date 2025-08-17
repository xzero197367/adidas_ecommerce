using Adidas.Application.Contracts.ServicesContracts;
using Adidas.Application.Services;
using Adidas.Context;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Models.People;
using System.Text;

namespace Adidas.ClientAPI
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // -------------------------------
            // Configure DbContext
            // -------------------------------
            builder.Services.AddDbContext<AdidasDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            // -------------------------------
            // Configure Identity
            // -------------------------------
            builder.Services.AddIdentity<User, IdentityRole>(options =>
            {
                options.Password.RequireDigit = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.Password.RequiredLength = 4;
            })
            .AddEntityFrameworkStores<AdidasDbContext>()
            .AddSignInManager()
            .AddApiEndpoints();

            // -------------------------------
            // Configure JWT Authentication
            // -------------------------------
            var secretKey = builder.Configuration["Authentication:JwtSettings:SecretKey"];
            var issuer = builder.Configuration["Authentication:JwtSettings:Issuer"];
            var audience = builder.Configuration["Authentication:JwtSettings:Audience"];

            // تحقق من SecretKey قبل الاستخدام
            if (string.IsNullOrEmpty(secretKey))
                throw new Exception("JWT SecretKey is null! تحقق من appsettings.json");

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = issuer,
                    ValidAudience = audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
                };

                options.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = context =>
                    {
                        Console.WriteLine($"Authentication failed: {context.Exception.Message}");
                        return Task.CompletedTask;
                    },
                    OnTokenValidated = context =>
                    {
                        Console.WriteLine($"Token validated successfully for user: {context.Principal?.Identity?.Name}");
                        return Task.CompletedTask;
                    }
                };
            });


            // -------------------------------
            // Add Controllers & Swagger
            // -------------------------------
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            // -------------------------------
            // Add Application Services (DI)
            // -------------------------------
            builder.Services.AddApplicationServices();
            builder.Services.AddScoped<IAuthService, AuthService>();

            // -------------------------------
            // Build App
            // -------------------------------
            var app = builder.Build();

            app.UseHttpsRedirection();
            app.UseAuthentication();
            app.UseAuthorization();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.MapControllers();
            app.Run();
        }
    }
}
