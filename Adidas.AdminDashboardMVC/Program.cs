using Adidas.Application.Contracts.RepositoriesContracts.Separator;
using Adidas.Application.Contracts.RepositoriesContracts;
using Adidas.Context;
using Adidas.Infra.Separator;
using Adidas.Infra;
using Microsoft.EntityFrameworkCore;

namespace Adidas.AdminDashboardMVC
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Configure DbContext
            builder.Services.AddDbContext<AdidasDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));



            // Add services to the container.
            builder.Services.AddControllersWithViews();
            builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
            builder.Services.AddScoped<IBrandRepository, BrandRepository>();
            builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();


            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
            }
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}
