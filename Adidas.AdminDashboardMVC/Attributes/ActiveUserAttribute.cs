using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Identity;
using Models.People;

namespace Adidas.AdminDashboardMVC.Attributes
{
    public class ActiveUserAttribute : ActionFilterAttribute
    {
        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var userManager = context.HttpContext.RequestServices.GetRequiredService<UserManager<User>>();
            var signInManager = context.HttpContext.RequestServices.GetRequiredService<SignInManager<User>>();

            if (context.HttpContext.User.Identity.IsAuthenticated)
            {
                var user = await userManager.GetUserAsync(context.HttpContext.User);

                if (user == null || !user.IsActive)
                {
                    await signInManager.SignOutAsync();
                    context.Result = new RedirectToActionResult("Login", "Account",
                        new { message = "Your account has been deactivated." });
                    return;
                }
            }

            await next();
        }
    }
}