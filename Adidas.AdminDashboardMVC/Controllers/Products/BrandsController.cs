  using Adidas.Application.Contracts.ServicesContracts.Separator;
using Adidas.DTOs.Separator.Brand_DTOs;
using Microsoft.AspNetCore.Mvc;

namespace Adidas.AdminDashboardMVC.Controllers.Products
{
    public class BrandsController : Controller
    {
        private readonly IBrandService _brandService;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public BrandsController(IBrandService brandService, IWebHostEnvironment webHostEnvironment)
        {
            _brandService = brandService;
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task<IActionResult> Index(string statusFilter, string searchTerm)
        {
            var brands = await _brandService.GetFilteredBrandsAsync(statusFilter, searchTerm);

            ViewData["CurrentStatus"] = statusFilter;
            ViewData["SearchTerm"] = searchTerm;
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

        [HttpGet]
        public IActionResult Create()
        {
            return View(new BrandCreateDto());
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BrandCreateDto createBrandDto, IFormFile? LogoImageFile)
        {
            if (!ModelState.IsValid)
            {
                return View(createBrandDto);
            }

            if (LogoImageFile != null && LogoImageFile.Length > 0)
            {
                if (LogoImageFile.Length > 5 * 1024 * 1024)
                {
                    ModelState.AddModelError("LogoUrl", "Image size should not exceed 5MB.");
                    return View(createBrandDto);
                }

                var fileName = Guid.NewGuid() + Path.GetExtension(LogoImageFile.FileName);
                var relativePath = Path.Combine("uploads", "brands", fileName);
                var absolutePath = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "brands");

                if (!Directory.Exists(absolutePath))
                {
                    Directory.CreateDirectory(absolutePath);
                }

                var filePath = Path.Combine(absolutePath, fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await LogoImageFile.CopyToAsync(stream);
                }

                createBrandDto.LogoUrl = "/" + relativePath.Replace("\\", "/");
            }

            var result = await _brandService.CreateAsync(createBrandDto);

            if (!result.IsSuccess)
            {
                ModelState.AddModelError(string.Empty, result.Error);
                TempData["Error"] = result.Error;
                return View(createBrandDto);
            }

            TempData["Success"] = "Brand created successfully!";
            return RedirectToAction("Index");
        }
        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
        {
            var brand = await _brandService.GetBrandToEditByIdAsync(id);

            if (brand == null)
            {
                TempData["Error"] = "Brand not found.";
                return RedirectToAction(nameof(Index));
            }


            var updateBrandDto = new BrandUpdateDto
            {
                Id = brand.Id,
                Name = brand.Name,
                Description = brand.Description,
                LogoUrl = brand.LogoUrl,

            };

            return View(updateBrandDto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(BrandUpdateDto updateBrandDto, IFormFile? LogoImageFile)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.BrandId = updateBrandDto.Id;
                return View(updateBrandDto);
            }

            if (LogoImageFile != null && LogoImageFile.Length > 0)
            {
                if (LogoImageFile.Length > 5 * 1024 * 1024)
                {
                    ModelState.AddModelError("LogoUrl", "Image size should not exceed 5MB.");
                    ViewBag.BrandId = updateBrandDto.Id;
                    return View(updateBrandDto);
                }

                var fileName = Guid.NewGuid() + Path.GetExtension(LogoImageFile.FileName);
                var relativePath = Path.Combine("uploads", "brands", fileName);
                var absolutePath = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "brands");

                if (!Directory.Exists(absolutePath))
                {
                    Directory.CreateDirectory(absolutePath);
                }

                var filePath = Path.Combine(absolutePath, fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await LogoImageFile.CopyToAsync(stream);
                }
                updateBrandDto.LogoUrl = "/" + relativePath.Replace("\\", "/");
            }

            var result = await _brandService.UpdateAsync(updateBrandDto);
            if (!result.IsSuccess)
            {
                ModelState.AddModelError(string.Empty, result.Error);
                TempData["Error"] = result.Error;
                ViewBag.BrandId = updateBrandDto.Id;
                return View(updateBrandDto);
            }

            TempData["Success"] = "Brand updated successfully!";
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Details(Guid id)
        {
            var brandDetails = await _brandService.GetDetailsByIdAsync(id);

            if (brandDetails == null)
            {
                TempData["Error"] = "Brand not found.";
                return RedirectToAction(nameof(Index));
            }

            return View(brandDetails);
        }

        public async Task<IActionResult> ToggleStatus(Guid id)
        {
            var result = await _brandService.ToggleBrandStatusAsync(id);

            if (!result.IsSuccess)
            {
                TempData["Error"] = result.Error;
            }
            else
            {
                TempData["Success"] = "Brands status updated successfully.";
            }

            return RedirectToAction(nameof(Index));
        }
        
        // api calls
        [HttpGet]
        public async Task<IActionResult> GetBrandsAJax()
        {
            try
            {
                var result = await _brandService.GetAllAsync();
                // if (!result.IsSuccess)
                // {
                //     return BadRequest(new { message = result.ErrorMessage });
                // }

                var brands = result.Data?.Select(b => new
                {
                    b.Id,
                    b.Name,
                    b.LogoUrl
                }).OrderBy(b => b.Name);

                return Ok(brands);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error retrieving brands", error = ex.Message });
            }
        }
    }
}
 