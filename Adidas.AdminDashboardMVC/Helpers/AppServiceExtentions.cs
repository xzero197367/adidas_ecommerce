using Adidas.Context;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Models.People;


namespace Resto.Web.Helpers
{
    public static class AppServiceExtentions
    {
        public static void InitializeDB(this IServiceCollection services, string connectionString)
        {
         
            // 2. Register your custom DbContext with Identity support
            services.AddDbContext<AdidasDbContext>(options =>
                options.UseSqlServer(connectionString));

            services.AddIdentity<User, IdentityRole>()
             .AddEntityFrameworkStores<AdidasDbContext>()
             .AddDefaultTokenProviders();

        }
        
    }
}
