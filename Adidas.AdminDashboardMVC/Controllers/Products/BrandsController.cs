using Adidas.Application.Contracts.ServicesContracts.Separator;
using Microsoft.AspNetCore.Mvc;

namespace Adidas.AdminDashboardMVC.Controllers.Products
{
    public class BrandsController : Controller
    {
        private readonly IBrandService _brandService;

        public BrandsController(IBrandService brandService)
        {
            _brandService = brandService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
             var brands = await _brandService.GetActiveBrandsAsync();

             return View(brands);
        }
    }
}
