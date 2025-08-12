using Adidas.AdminDashboardMVC.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Adidas.AdminDashboardMVC.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        

        [Route("Home/Error")]
        public IActionResult Error()
        {
            return View("~/Views/Shared/Error.cshtml");
        }
        [Route("Home/HandleError/{statusCode}")]
        public IActionResult HandleError(int statusCode)
        {
            if (statusCode == 404)
            {
                return View("~/Views/Shared/NotFound.cshtml");
            }
            return View("~/Views/Shared/Error.cshtml");
        }
    }
}
