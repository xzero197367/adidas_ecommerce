using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Adidas.AdminDashboardMVC.Attributes;
using System.Security.Claims;

namespace Adidas.AdminDashboardMVC.Controllers
{
    [Authorize]
    [ActiveUser]
    public abstract class BaseController : Controller
    {
        protected string GetCurrentUserId()
        {
            return User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }

        protected string GetCurrentUserEmail()
        {
            return User.FindFirst(ClaimTypes.Email)?.Value;
        }

        protected bool IsUserInRole(string role)
        {
            return User.IsInRole(role);
        }

        protected bool IsAdmin()
        {
            return User.IsInRole("Admin");
        }

        protected bool IsEmployee()
        {
            return User.IsInRole("Employee");
        }
    }
}
