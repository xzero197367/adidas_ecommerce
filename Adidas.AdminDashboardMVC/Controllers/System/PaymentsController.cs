using Microsoft.AspNetCore.Mvc;

namespace Adidas.AdminDashboardMVC.Controllers.System
{
    public class PaymentsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
