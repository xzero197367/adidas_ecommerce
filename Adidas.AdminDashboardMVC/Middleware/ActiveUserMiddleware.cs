using Microsoft.AspNetCore.Identity;
using Models.People;
using System.Security.Claims;

namespace Adidas.AdminDashboardMVC.Middleware
{
    public class ActiveUserMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public ActiveUserMiddleware(RequestDelegate next, IServiceScopeFactory serviceScopeFactory)
        {
            _next = next;
            _serviceScopeFactory = serviceScopeFactory;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (context.User.Identity.IsAuthenticated)
            {
                using var scope = _serviceScopeFactory.CreateScope();
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
                var signInManager = scope.ServiceProvider.GetRequiredService<SignInManager<User>>();

                var user = await userManager.GetUserAsync(context.User);

                if (user != null && !user.IsActive)
                {
                    await signInManager.SignOutAsync();
                    context.Response.Redirect("/Account/Login?message=account-deactivated");
                    return;
                }
            }

            await _next(context);
        }
    }

    // Extension method for easier registration
    public static class ActiveUserMiddlewareExtensions
    {
        public static IApplicationBuilder UseActiveUserMiddleware(
            this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ActiveUserMiddleware>();
        }
    }
}