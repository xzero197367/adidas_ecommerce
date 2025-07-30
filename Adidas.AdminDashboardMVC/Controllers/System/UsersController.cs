using Microsoft.AspNetCore.Mvc;

namespace Adidas.AdminDashboardMVC.Controllers.System
{
    public class UsersController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
