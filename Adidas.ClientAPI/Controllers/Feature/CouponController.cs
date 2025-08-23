using Adidas.Application.Contracts.ServicesContracts.Feature;
using Adidas.DTOs.Feature.CouponDTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Adidas.API.Controllers.Feature
{
    [ApiController]
    [Route("api/[controller]")]
    
    public class CouponController : ControllerBase
    {
        private readonly ICouponService _couponService;
        private readonly ILogger<CouponController> _logger;

        public CouponController(ICouponService couponService, ILogger<CouponController> logger)
        {
            _couponService = couponService;
            _logger = logger;
        }

        /// <summary>
        /// Get filtered and paged list of coupons
        /// </summary>
        /// <param name="search">Search term for coupon code</param>
        /// <param name="status">Filter by status: active, inactive, expired, all</param>
        /// <param name="page">Page number (default: 1)</param>
        /// <param name="pageSize">Items per page (default: 10)</param>
        /// <returns>Paged coupon list with statistics</returns>
        [HttpGet]
        [ProducesResponseType(typeof(CouponListResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetCoupons(
            [FromQuery] string? search = null,
            [FromQuery] string? status = "all",
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                if (page < 1) page = 1;
                if (pageSize < 1 || pageSize > 100) pageSize = 10;

                var result = await _couponService.GetFilteredPagedCouponsAsync(
                    search ?? string.Empty,
                    status ?? "all",
                    page,
                    pageSize);

                return Ok(new
                {
                    Success = true,
                    Message = "Coupons retrieved successfully",
                    Data = result,
                    Pagination = new
                    {
                        CurrentPage = page,
                        PageSize = pageSize,
                        TotalItems = result.TotalCount,
                        TotalPages = (int)Math.Ceiling((double)result.TotalCount / pageSize)
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving coupons with search: {Search}, status: {Status}", search, status);
                return StatusCode(500, new
                {
                    Success = false,
                    Message = "An error occurred while retrieving coupons",
                    Error = ex.Message
                });
            }
        }

        /// <summary>
        /// Get coupon details by ID
        /// </summary>
        /// <param name="id">Coupon ID</param>
        /// <returns>Detailed coupon information including usage statistics</returns>
        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(CouponDetailsDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetCouponDetails(Guid id)
        {
            try
            {
                var couponDetails = await _couponService.GetCouponDetailsByIdAsync(id);

                if (couponDetails.CouponDto == null)
                {
                    return NotFound(new
                    {
                        Success = false,
                        Message = "Coupon not found"
                    });
                }

                return Ok(new
                {
                    Success = true,
                    Message = "Coupon details retrieved successfully",
                    Data = couponDetails
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving coupon details for ID: {Id}", id);
                return StatusCode(500, new
                {
                    Success = false,
                    Message = "An error occurred while retrieving coupon details",
                    Error = ex.Message
                });
            }
        }

        /// <summary>
        /// Get coupon for editing
        /// </summary>
        /// <param name="id">Coupon ID</param>
        /// <returns>Coupon data formatted for editing</returns>
        [HttpGet("{id:guid}/edit")]
        [ProducesResponseType(typeof(CouponUpdateDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetCouponForEdit(Guid id)
        {
            try
            {
                var couponToEdit = await _couponService.GetCouponToEditByIdAsync(id);

                if (couponToEdit == null)
                {
                    return NotFound(new
                    {
                        Success = false,
                        Message = "Coupon not found"
                    });
                }

                couponToEdit.Id = id; // Ensure ID is set

                return Ok(new
                {
                    Success = true,
                    Message = "Coupon retrieved for editing successfully",
                    Data = couponToEdit
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving coupon for edit with ID: {Id}", id);
                return StatusCode(500, new
                {
                    Success = false,
                    Message = "An error occurred while retrieving coupon for editing",
                    Error = ex.Message
                });
            }
        }

        /// <summary>
        /// Create a new coupon
        /// </summary>
        /// <param name="createDto">Coupon creation data</param>
        /// <returns>Creation result</returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateCoupon([FromBody] CouponCreateDto createDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new
                    {
                        Success = false,
                        Message = "Invalid data provided",
                        Errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)
                    });
                }

                var result = await _couponService.CreateAsync(createDto);

                if (!result.IsSuccess)
                {
                    // Check if it's a duplicate code error
                    if (result.Error.Contains("already exists"))
                    {
                        return Conflict(new
                        {
                            Success = false,
                            Message = result.Error
                        });
                    }

                    return BadRequest(new
                    {
                        Success = false,
                        Message = result.Error
                    });
                }

                return CreatedAtAction(
                    nameof(GetCouponDetails),
                    new { id = Guid.NewGuid() }, // This would ideally be the actual created coupon ID
                    new
                    {
                        Success = true,
                        Message = "Coupon created successfully"
                    });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating coupon with code: {Code}", createDto.Code);
                return StatusCode(500, new
                {
                    Success = false,
                    Message = "An error occurred while creating the coupon",
                    Error = ex.Message
                });
            }
        }

        /// <summary>
        /// Update an existing coupon
        /// </summary>
        /// <param name="id">Coupon ID</param>
        /// <param name="updateDto">Coupon update data</param>
        /// <returns>Update result</returns>
        [HttpPut("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateCoupon(Guid id, [FromBody] CouponUpdateDto updateDto)
        {
            try
            {
                if (id != updateDto.Id)
                {
                    return BadRequest(new
                    {
                        Success = false,
                        Message = "ID in URL does not match ID in request body"
                    });
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(new
                    {
                        Success = false,
                        Message = "Invalid data provided",
                        Errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)
                    });
                }

                var result = await _couponService.UpdateAsync(updateDto);

                if (!result.IsSuccess)
                {
                    if (result.Error.Contains("not found"))
                    {
                        return NotFound(new
                        {
                            Success = false,
                            Message = result.Error
                        });
                    }

                    if (result.Error.Contains("already in use"))
                    {
                        return Conflict(new
                        {
                            Success = false,
                            Message = result.Error
                        });
                    }

                    return BadRequest(new
                    {
                        Success = false,
                        Message = result.Error
                    });
                }

                return Ok(new
                {
                    Success = true,
                    Message = "Coupon updated successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating coupon with ID: {Id}", id);
                return StatusCode(500, new
                {
                    Success = false,
                    Message = "An error occurred while updating the coupon",
                    Error = ex.Message
                });
            }
        }

        /// <summary>
        /// Toggle coupon active status
        /// </summary>
        /// <param name="id">Coupon ID</param>
        /// <returns>Toggle result</returns>
        [HttpPatch("{id:guid}/toggle-status")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ToggleCouponStatus(Guid id)
        {
            try
            {
                var result = await _couponService.ToggleCouponStatusAsync(id);

                if (!result.IsSuccess)
                {
                    if (result.Error.Contains("not found"))
                    {
                        return NotFound(new
                        {
                            Success = false,
                            Message = result.Error                    });
                    }

                    return BadRequest(new
                    {
                        Success = false,
                        Message = result.Error
                    });
                }

                return Ok(new
                {
                    Success = true,
                    Message = "Coupon status toggled successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling coupon status for ID: {Id}", id);
                return StatusCode(500, new
                {
                    Success = false,
                    Message = "An error occurred while toggling coupon status",
                    Error = ex.Message
                });
            }
        }

        /// <summary>
        /// Soft delete a coupon
        /// </summary>
        /// <param name="id">Coupon ID</param>
        /// <returns>Deletion result</returns>
        [HttpDelete("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteCoupon(Guid id)
        {
            try
            {
                var result = await _couponService.SoftDeletAsync(id);

                if (!result.IsSuccess)
                {
                    if (result.Error.Contains("not found"))
                    {
                        return NotFound(new
                        {
                            Success = false,
                            Message = result.Error
                        });
                    }

                    return BadRequest(new
                    {
                        Success = false,
                        Message = result.Error
                    });
                }

                return Ok(new
                {
                    Success = true,
                    Message = "Coupon deleted successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting coupon with ID: {Id}", id);
                return StatusCode(500, new
                {
                    Success = false,
                    Message = "An error occurred while deleting the coupon",
                    Error = ex.Message
                });
            }
        }

        /// <summary>
        /// Apply coupon to cart (for validation before order creation)
        /// </summary>
        /// <param name="request">Apply coupon request</param>
        /// <returns>Coupon application result with discount details</returns>
        [HttpPost("apply-to-cart")]
        [ProducesResponseType(typeof(CouponApplicationResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ApplyCouponToCart([FromBody] ApplyCouponToCartRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new
                    {
                        Success = false,
                        Message = "Invalid request data",
                        Errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)
                    });
                }

                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new
                    {
                        Success = false,
                        Message = "User not authenticated"
                    });
                }

                var result = await _couponService.ApplyCouponToCartAsync(userId, request.CouponCode, request.CartTotal);

                return Ok(new
                {
                    Success = result.Success,
                    Message = result.Success ? "Coupon applied successfully" : result.Message,
                    Data = result.Success ? new
                    {
                        DiscountApplied = result.DiscountApplied,
                        NewTotal = result.NewTotal,
                        OriginalTotal = request.CartTotal
                    } : null
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error applying coupon {Code} to cart", request.CouponCode);
                return StatusCode(500, new
                {
                    Success = false,
                    Message = "An error occurred while applying the coupon",
                    Error = ex.Message
                });
            }
        }

        /// <summary>
        /// Apply coupon to an existing order
        /// </summary>
        /// <param name="request">Apply coupon to order request</param>
        /// <returns>Coupon application result</returns>
        [HttpPost("apply-to-order")]
        [ProducesResponseType(typeof(CouponApplicationResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ApplyCouponToOrder([FromBody] ApplyCouponRequestDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new
                    {
                        Success = false,
                        Message = "Invalid request data",
                        Errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)
                    });
                }

                var result = await _couponService.ApplyCouponToOrderAsync(request.OrderId, request.CouponCode);

                if (!result.Success)
                {
                    if (result.Message.Contains("not found"))
                    {
                        return NotFound(new
                        {
                            Success = false,
                            Message = result.Message
                        });
                    }

                    return BadRequest(new
                    {
                        Success = false,
                        Message = result.Message
                    });
                }

                return Ok(new
                {
                    Success = true,
                    Message = "Coupon applied to order successfully",
                    Data = new
                    {
                        DiscountApplied = result.DiscountApplied,
                        NewTotal = result.NewTotal
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error applying coupon {Code} to order {OrderId}", request.CouponCode, request.OrderId);
                return StatusCode(500, new
                {
                    Success = false,
                    Message = "An error occurred while applying coupon to order",
                    Error = ex.Message
                });
            }
        }

        /// <summary>
        /// Calculate coupon discount amount for a given order amount
        /// </summary>
        /// <param name="code">Coupon code</param>
        /// <param name="orderAmount">Order amount</param>
        /// <returns>Calculated discount amount</returns>
        [HttpGet("calculate-discount")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CalculateCouponDiscount(
            [FromQuery] string code,
            [FromQuery] decimal orderAmount)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(code))
                {
                    return BadRequest(new
                    {
                        Success = false,
                        Message = "Coupon code is required"
                    });
                }

                if (orderAmount <= 0)
                {
                    return BadRequest(new
                    {
                        Success = false,
                        Message = "Order amount must be greater than zero"
                    });
                }

                var discountAmount = await _couponService.CalculateCouponAmountAsync(code, orderAmount);

                if (discountAmount == -1)
                {
                    return StatusCode(500, new
                    {
                        Success = false,
                        Message = "An error occurred while calculating discount"
                    });
                }

                return Ok(new
                {
                    Success = true,
                    Message = discountAmount > 0 ? "Discount calculated successfully" : "No discount available for this coupon",
                    Data = new
                    {
                        CouponCode = code,
                        OrderAmount = orderAmount,
                        DiscountAmount = discountAmount,
                        FinalAmount = orderAmount - discountAmount
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating discount for coupon {Code} with amount {Amount}", code, orderAmount);
                return StatusCode(500, new
                {
                    Success = false,
                    Message = "An error occurred while calculating discount",
                    Error = ex.Message
                });
            }
        }
    }

    // Additional request DTOs
    public class ApplyCouponToCartRequest
    {
        public string CouponCode { get; set; } = string.Empty;
        public decimal CartTotal { get; set; }
    }
}