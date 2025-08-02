using Microsoft.AspNetCore.Mvc;

namespace Adidas.AdminDashboardMVC.Controllers.Content
{
    public class BannersController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
