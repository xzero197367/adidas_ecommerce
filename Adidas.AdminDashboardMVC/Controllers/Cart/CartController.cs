using Microsoft.AspNetCore.Mvc;

namespace Adidas.AdminDashboardMVC.Controllers.Cart
{
    public class CartController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
