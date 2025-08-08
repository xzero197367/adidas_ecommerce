using Adidas.Application.Contracts.ServicesContracts.Feature;
using Adidas.DTOs.Feature.CouponDTOs;
using Microsoft.AspNetCore.Mvc;
using Adidas.Context;
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
        public async Task<IActionResult> Index(string search = "", string status = "All", int page = 1, int pageSize = 10)
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

            return View(result.Coupons);
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
        public async Task<IActionResult> Edit(Guid id)
        {
            var coupon = await _couponService.GetByIdAsync(id);
            if (coupon == null)
                return NotFound();

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
                UsedCount = coupon.UsedCount,
                IsActive = coupon.IsActive
            };

            ViewBag.CouponId = id;
            return View(updateDto);
        }

        // POST: Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, CouponUpdateDto dto)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.CouponId = id;
                return View(dto);
            }

            try
            {
                await _couponService.UpdateAsync(id, dto);
                TempData["Success"] = "Coupon updated successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error updating coupon: {ex.Message}";
                ViewBag.CouponId = id;
                return View(dto);
            }
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

        /*
                // POST: Activate (using UpdateAsync with IsActive = true)
                [HttpPost]
                [ValidateAntiForgeryToken]
                public async Task<IActionResult> Activate(Guid id)
                {
                    try
                    {
                        var coupon = await _couponService.GetByIdAsync(id);
                        if (coupon == null)
                            return NotFound();

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
                            UsedCount = coupon.UsedCount,
                            IsActive = true
                        };

                        await _couponService.UpdateAsync(id, updateDto);
                        TempData["Success"] = "Coupon activated successfully!";
                    }
                    catch (Exception ex)
                    {
                        TempData["Error"] = $"Error activating coupon: {ex.Message}";
                    }

                    return RedirectToAction(nameof(Index));
                }

                // POST: Deactivate (using UpdateAsync with IsActive = false)
                [HttpPost]
                [ValidateAntiForgeryToken]
                public async Task<IActionResult> Deactivate(Guid id)
                {
                    try
                    {
                        var coupon = await _couponService.GetByIdAsync(id);
                        if (coupon == null)
                            return NotFound();

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
                            UsedCount = coupon.UsedCount,
                            IsActive = false
                        };

                        await _couponService.UpdateAsync(id, updateDto);
                        TempData["Success"] = "Coupon deactivated successfully!";
                    }
                    catch (Exception ex)
                    {
                        TempData["Error"] = $"Error deactivating coupon: {ex.Message}";
                    }

                    return RedirectToAction(nameof(Index));
                }

                // AJAX endpoint for quick actions
                [HttpPost]
                public async Task<IActionResult> ToggleStatus(Guid id)
                {
                    var coupon = await _context.Coupons.FindAsync(id);
                    if (coupon == null)
                        return Json(new { success = false, message = "Coupon not found" });

                    try
                    {
                        // Toggle logic
                        if (coupon.ValidTo > DateTime.Now)
                        {
                            coupon.ValidTo = DateTime.Now.AddDays(-1); // deactivate
                        }
                        else
                        {
                            coupon.ValidTo = DateTime.Now.AddDays(30); // activate
                        }

                        coupon.UpdatedAt = DateTime.Now;

                        await _context.SaveChangesAsync();

                        var isActive = coupon.ValidTo > DateTime.Now && coupon.UsedCount < coupon.UsageLimit;

                        return Json(new
                        {
                            success = true,
                            status = isActive ? "Active" : "Inactive",
                            message = $"Coupon {coupon.Code} status updated successfully!"
                        });
                    }
                    catch (Exception ex)
                    {
                        return Json(new
                        {
                            success = false,
                            message = $"Error: {ex.Message}",
                            details = ex.InnerException?.Message
                        });
                    }
                }
                */

        // Helper method for calculating total savings
        //private decimal CalculateTotalSavings(List<CouponDto> coupons)
        //{
        //    return coupons.Sum(c =>
        //    {
        //        if (c.DiscountType.ToString().Equals("Percentage", StringComparison.OrdinalIgnoreCase))
        //        {
        //            // For percentage, estimate based on average order value
        //            // You might want to get actual order data here
        //            decimal avgOrderValue = 100; // Adjust based on your business
        //            return c.UsedCount * (avgOrderValue * c.DiscountValue / 100);
        //        }
        //        else
        //        {
        //            // Fixed amount
        //            return c.UsedCount * c.DiscountValue;
        //        }
        //    });
    }
}
