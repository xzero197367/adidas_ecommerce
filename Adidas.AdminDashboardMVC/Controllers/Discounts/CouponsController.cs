using Adidas.Application.Contracts.ServicesContracts.Feature;
using Adidas.DTOs.Feature.CouponDTOs;
using Microsoft.AspNetCore.Mvc;

namespace Adidas.Web.Controllers
{
    public class CouponController : Controller
    {
        private readonly ICouponService _couponService;

        public CouponController(ICouponService couponService)
        {
            _couponService = couponService;
        }

        // Display all coupons with filters, search, and pagination
        public async Task<IActionResult> Index(string search = "", string status = "All", int page = 1, int pageSize = 10)
        {
            var result = await _couponService.GetAllCouponsAsync();
            var allCoupons = result.Data;

            // Filtering by search
            if (!string.IsNullOrWhiteSpace(search))
                allCoupons = allCoupons.Where(c => c.Code.Contains(search, StringComparison.OrdinalIgnoreCase));

            // Filter by status
            if (!string.IsNullOrWhiteSpace(status) && status != "All")
            {
                allCoupons = allCoupons.Where(c => c.StatusText.Equals(status, StringComparison.OrdinalIgnoreCase));
            }

            // Stats
            ViewBag.TotalUsage = allCoupons.Sum(c => c.UsedCount);
            ViewBag.TotalSavings = allCoupons.Sum(c => c.UsedCount * c.DiscountValue); // Approximation
            ViewBag.ActiveCount = allCoupons.Count(c => c.IsValidNow);
            ViewBag.ExpiredCount = allCoupons.Count(c => c.IsExpired);

            // Pagination
            int totalCoupons = allCoupons.Count();
            var pagedCoupons = allCoupons
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalCoupons / pageSize);
            ViewBag.Search = search;
            ViewBag.Status = status;

            return View(pagedCoupons);
        }

        // GET: Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CouponCreateDto dto)
        {
            if (!ModelState.IsValid)
                return View(dto);

            await _couponService.CreateAsync(dto);
            return RedirectToAction(nameof(Index));
        }

        // GET: Edit
        public async Task<IActionResult> Edit(Guid id)
        {
            var result = await _couponService.GetByIdAsync(id);
            if (result.IsSuccess == false)
                return NotFound();
            var coupon = result.Data;

            var updateDto = new CouponUpdateDto
            {
                Code = coupon.Code,
                Name = coupon.Name,
                DiscountType = coupon.DiscountType,
                DiscountValue = coupon.DiscountValue,
                MinimumAmount = coupon.MinimumAmount,
                ValidFrom = coupon.ValidFrom,
                ValidTo = coupon.ValidTo,
                UsageLimit = coupon.UsageLimit,
                // UsedCount = coupon.UsedCount
            };

            ViewBag.CouponId = id; // for form submission
            return View(updateDto);
        }

        // POST: Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, CouponUpdateDto dto)
        {
            if (!ModelState.IsValid)
                return View(dto);

            await _couponService.UpdateAsync(dto);
            return RedirectToAction(nameof(Index));
        }

        // POST: Delete
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _couponService.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
