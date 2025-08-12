using Adidas.Application.Contracts.ServicesContracts.Feature;
using Adidas.DTOs.Feature.CouponDTOs;
using Microsoft.AspNetCore.Mvc;
using Adidas.Context;
using Adidas.DTOs.Common_DTOs;

namespace Adidas.Web.Controllers
{
    public class CouponController : Controller
    {
        private readonly ICouponService _couponService;
        private readonly AdidasDbContext _context;

        public CouponController(ICouponService couponService)
        {
            _couponService = couponService;
        }

        // Display all coupons with filters, search, and pagination
        public async Task<IActionResult> Index(string search = "", string status = "All", int page = 1,
            int pageSize = 10)
        {
            var result = await _couponService.GetFilteredPagedCouponsAsync(search, status, page, pageSize);
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling((double)result.TotalCount / pageSize);
            ViewBag.Search = search;
            ViewBag.Status = status;
            ViewBag.PageSize = pageSize;

            ViewBag.TotalUsage = result.TotalUsage;
            ViewBag.TotalSavings = result.TotalSavings;
            ViewBag.ActiveCount = result.ActiveCount;
            ViewBag.ExpiredCount = result.ExpiredCount;

            return View(result.Coupons.ToList());
        }

        // GET: Create
        public IActionResult Create()
        {
            return View(new CouponCreateDto());
        }

        // POST: Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CouponCreateDto dto)
        {
            if (!ModelState.IsValid)
            {
                return View(dto);
            }

            var result = await _couponService.CreateAsync(dto);

            if (!result.IsSuccess)
            {
                TempData["Error"] = result.Error;
                return View(dto);
            }

            TempData["Success"] = "Coupon created successfully!";
            return RedirectToAction(nameof(Index));
        }


        // GET: Edit
        public async Task<IActionResult> Details(Guid id)
        {
            var result = await _couponService.GetCouponDetailsByIdAsync(id);

            if (result == null)
                return NotFound();


            return View(result);
        }

        public async Task<IActionResult> Edit(Guid id)
        {
            var couponUpdateDto = await _couponService.GetCouponToEditByIdAsync(id);
            if (couponUpdateDto == null)
                return NotFound();


            return View(couponUpdateDto);
        }

        // POST: Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(CouponUpdateDto dto)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.CouponId = dto.Id;
                return View(dto);
            }

            var result = await _couponService.UpdateAsync(dto);
            if (!result.IsSuccess)
            {
                ModelState.AddModelError(string.Empty, result.Error);
                TempData["Error"] = result.Error;
                ViewBag.CouponId = dto.Id;

                return View(dto);
            }

            TempData["Success"] = "Coupon updated successfully!";

            return RedirectToAction(nameof(Index));
        }

        // POST: Delete
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id)
        {
            var result = await _couponService.SoftDeletAsync(id);

            if (!result.IsSuccess)
            {
                TempData["Error"] = result.Error;
            }
            else
            {
                TempData["Success"] = "Category deleted successfully!";
            }

            var referer = Request.Headers["Referer"].ToString();

            if (!string.IsNullOrEmpty(referer))
            {
                return Redirect(referer);
            }

            return RedirectToAction(nameof(Index));
        }


        [HttpPost]
        public async Task<IActionResult> ToggleStatus(Guid id)
        {
            var result = await _couponService.ToggleCouponStatusAsync(id);

            if (!result.IsSuccess)
            {
                TempData["Error"] = result.Error;
            }
            else
            {
                TempData["Success"] = "coupon status updated successfully.";
            }

            var referer = Request.Headers["Referer"].ToString();
            if (!string.IsNullOrEmpty(referer))
            {
                return Redirect(referer);
            }


            return RedirectToAction(nameof(Index));
        }
    }
}