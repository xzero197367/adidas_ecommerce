using Microsoft.AspNetCore.Mvc;

namespace Adidas.AdminDashboardMVC.Controllers.Discounts
{
    public class CouponsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
