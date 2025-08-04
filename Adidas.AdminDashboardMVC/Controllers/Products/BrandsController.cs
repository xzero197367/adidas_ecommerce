using Adidas.Application.Contracts.ServicesContracts.Separator;
using Adidas.Application.Services.Separator;
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id)
        {
            var result = await _brandService.DeleteAsync(id);

            if (!result.IsSuccess)
            {
                TempData["Error"] = result.Error;
            }
            else
            {
                TempData["Success"] = "Brand deleted successfully!";
            }

            return RedirectToAction("Index");
        }

    }
}
