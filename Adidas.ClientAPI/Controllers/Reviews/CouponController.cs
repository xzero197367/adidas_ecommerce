using Adidas.Application.Contracts.ServicesContracts.Feature;
using Adidas.Application.Services.Feature;
using Adidas.DTOs.Feature.CouponDTOs;
using Adidas.DTOs.Feature.OrderCouponDTOs;
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
        private readonly IOrderCouponService _orderCouponService;
        private readonly ILogger<CouponController> _logger;

        public CouponController(ICouponService couponService, IOrderCouponService orderCouponService, ILogger<CouponController> logger)
        {
            _couponService = couponService;
            _orderCouponService = orderCouponService;
            _logger = logger;
        }


        [HttpGet("order-coupons")]
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


        /// <summary>
        /// Get all order coupons with related data
        /// </summary>
        /// <returns>List of order coupons with coupon and order details</returns>
        [HttpGet("GetAllOrderCoupons")]
        [ProducesResponseType(typeof(IEnumerable<OrderCouponDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllOrderCoupons()
        {
            try
            {
                var result = await _orderCouponService.GetWithIncludesAsync();

                if (!result.IsSuccess)
                {
                    return StatusCode(500, new
                    {
                        Success = false,
                        Message = result.ErrorMessage
                    });
                }

                return Ok(new
                {
                    Success = true,
                    Message = "Order coupons retrieved successfully",
                    Data = result.Data,
                    Count = result.Data?.Count() ?? 0
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all order coupons");
                return StatusCode(500, new
                {
                    Success = false,
                    Message = "An error occurred while retrieving order coupons",
                    Error = ex.Message
                });
            }
        }

        /// <summary>
        /// Get order coupon by ID
        /// </summary>
        /// <param name="id">Order coupon ID</param>
        /// <returns>Order coupon details</returns>
        [HttpGet("GetOrderCouponById/{id:guid}")]
        [ProducesResponseType(typeof(OrderCouponDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetOrderCouponById(Guid id)
        {
            try
            {
                var result = await _orderCouponService.GetByIdAsync(id);

                if (!result.IsSuccess)
                {
                    if (result.ErrorMessage.Contains("not found") || result.Data == null)
                    {
                        return NotFound(new
                        {
                            Success = false,
                            Message = "Order coupon not found"
                        });
                    }

                    return StatusCode(500, new
                    {
                        Success = false,
                        Message = result.ErrorMessage
                    });
                }

                return Ok(new
                {
                    Success = true,
                    Message = "Order coupon retrieved successfully",
                    Data = result.Data
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving order coupon with ID: {Id}", id);
                return StatusCode(500, new
                {
                    Success = false,
                    Message = "An error occurred while retrieving order coupon",
                    Error = ex.Message
                });
            }
        }

        /// <summary>
        /// Get all coupons applied to a specific order
        /// </summary>
        /// <param name="orderId">Order ID</param>
        /// <returns>List of coupons applied to the order</returns>
        [HttpGet("GetCouponsByOrderId/{orderId:guid}")]
        [ProducesResponseType(typeof(IEnumerable<OrderCouponDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetCouponsByOrderId(Guid orderId)
        {
            try
            {
                var result = await _orderCouponService.GetByOrderIdAsync(orderId);

                if (!result.IsSuccess)
                {
                    return StatusCode(500, new
                    {
                        Success = false,
                        Message = result.ErrorMessage
                    });
                }

                return Ok(new
                {
                    Success = true,
                    Message = "Order coupons retrieved successfully",
                    Data = result.Data,
                    OrderId = orderId,
                    Count = result.Data?.Count() ?? 0,
                    TotalDiscount = result.Data?.Sum(oc => oc.DiscountApplied) ?? 0
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving coupons for order ID: {OrderId}", orderId);
                return StatusCode(500, new
                {
                    Success = false,
                    Message = "An error occurred while retrieving order coupons",
                    Error = ex.Message
                });
            }
        }

        /// <summary>
        /// Get order coupon by order ID and coupon ID
        /// </summary>
        /// <param name="orderId">Order ID</param>
        /// <param name="couponId">Coupon ID</param>
        /// <returns>Specific order coupon record</returns>
        [HttpGet("order/{orderId:guid}/coupon/{couponId:guid}")]
        [ProducesResponseType(typeof(OrderCouponDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetOrderCouponByOrderAndCouponId(Guid orderId, Guid couponId)
        {
            try
            {
                var result = await _orderCouponService.GetByOrderAndCouponIdAsync(orderId, couponId);

                if (!result.IsSuccess || result.Data == null)
                {
                    return NotFound(new
                    {
                        Success = false,
                        Message = "Order coupon combination not found"
                    });
                }

                return Ok(new
                {
                    Success = true,
                    Message = "Order coupon retrieved successfully",
                    Data = result.Data
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving order coupon for order ID: {OrderId} and coupon ID: {CouponId}", orderId, couponId);
                return StatusCode(500, new
                {
                    Success = false,
                    Message = "An error occurred while retrieving order coupon",
                    Error = ex.Message
                });
            }
        }

        /// <summary>
        /// Get total discount applied by a specific coupon across all orders
        /// </summary>
        /// <param name="couponId">Coupon ID</param>
        /// <returns>Total discount amount applied by the coupon</returns>
        [HttpGet("coupon/{couponId:guid}/total-discount")]
        [ProducesResponseType(typeof(decimal), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetTotalDiscountByCoupon(Guid couponId)
        {
            try
            {
                var result = await _orderCouponService.GetTotalDiscountAppliedByCouponAsync(couponId);

                if (!result.IsSuccess)
                {
                    return StatusCode(500, new
                    {
                        Success = false,
                        Message = result.ErrorMessage
                    });
                }

                return Ok(new
                {
                    Success = true,
                    Message = "Total discount calculated successfully",
                    Data = new
                    {
                        CouponId = couponId,
                        TotalDiscountApplied = result.Data
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating total discount for coupon ID: {CouponId}", couponId);
                return StatusCode(500, new
                {
                    Success = false,
                    Message = "An error occurred while calculating total discount",
                    Error = ex.Message
                });
            }
        }

        /// <summary>
        /// Get usage count for a specific coupon
        /// </summary>
        /// <param name="couponId">Coupon ID</param>
        /// <returns>Number of times the coupon has been used</returns>
        [HttpGet("coupon/{couponId:guid}/usage-count")]
        [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetCouponUsageCount(Guid couponId)
        {
            try
            {
                var result = await _orderCouponService.GetCouponUsageCountAsync(couponId);

                if (!result.IsSuccess)
                {
                    return StatusCode(500, new
                    {
                        Success = false,
                        Message = result.ErrorMessage
                    });
                }

                return Ok(new
                {
                    Success = true,
                    Message = "Coupon usage count retrieved successfully",
                    Data = new
                    {
                        CouponId = couponId,
                        UsageCount = result.Data
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving usage count for coupon ID: {CouponId}", couponId);
                return StatusCode(500, new
                {
                    Success = false,
                    Message = "An error occurred while retrieving coupon usage count",
                    Error = ex.Message
                });
            }
        }

        /// <summary>
        /// Create a new order coupon record
        /// </summary>
        /// <param name="createDto">Order coupon creation data</param>
        /// <returns>Creation result</returns>
        [HttpPost("CreateOrderCoupon")]
        [ProducesResponseType(typeof(OrderCouponDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateOrderCoupon([FromBody] OrderCouponCreateDto createDto)
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

                var result = await _orderCouponService.CreateAsync(createDto);

                if (!result.IsSuccess)
                {
                    return BadRequest(new
                    {
                        Success = false,
                        Message = result.ErrorMessage
                    });
                }

                return CreatedAtAction(
                    nameof(GetOrderCouponById),
                    new { id = result.Data.Id },
                    new
                    {
                        Success = true,
                        Message = "Order coupon created successfully",
                        Data = result.Data
                    });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating order coupon for Order: {OrderId}, Coupon: {CouponId}",
                    createDto.OrderId, createDto.CouponId);
                return StatusCode(500, new
                {
                    Success = false,
                    Message = "An error occurred while creating order coupon",
                    Error = ex.Message
                });
            }
        }

        /// <summary>
        /// Update an existing order coupon
        /// </summary>
        /// <param name="id">Order coupon ID</param>
        /// <param name="updateDto">Order coupon update data</param>
        /// <returns>Update result</returns>
        [HttpPut("UpdateOrderCoupon/{id:guid}")]
        [ProducesResponseType(typeof(OrderCouponDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateOrderCoupon(Guid id, [FromBody] OrderCouponUpdateDto updateDto)
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

                var result = await _orderCouponService.UpdateAsync(updateDto);

                if (!result.IsSuccess)
                {
                    if (result.ErrorMessage.Contains("not found"))
                    {
                        return NotFound(new
                        {
                            Success = false,
                            Message = result.ErrorMessage
                        });
                    }

                    return BadRequest(new
                    {
                        Success = false,
                        Message = result.ErrorMessage
                    });
                }

                return Ok(new
                {
                    Success = true,
                    Message = "Order coupon updated successfully",
                    Data = result.Data
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating order coupon with ID: {Id}", id);
                return StatusCode(500, new
                {
                    Success = false,
                    Message = "An error occurred while updating order coupon",
                    Error = ex.Message
                });
            }
        }

        /// <summary>
        /// Delete an order coupon record
        /// </summary>
        /// <param name="id">Order coupon ID</param>
        /// <returns>Deletion result</returns>
        [HttpDelete("DeleteOrderCoupon/{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteOrderCoupon(Guid id)
        {
            try
            {
                var result = await _orderCouponService.DeleteAsync(id);

                if (!result.IsSuccess)
                {
                    if (result.ErrorMessage.Contains("not found"))
                    {
                        return NotFound(new
                        {
                            Success = false,
                            Message = "Order coupon not found"
                        });
                    }

                    return BadRequest(new
                    {
                        Success = false,
                        Message = result.ErrorMessage
                    });
                }

                return Ok(new
                {
                    Success = true,
                    Message = "Order coupon deleted successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting order coupon with ID: {Id}", id);
                return StatusCode(500, new
                {
                    Success = false,
                    Message = "An error occurred while deleting order coupon",
                    Error = ex.Message
                });
            }
        }

        /// <summary>
        /// Get coupon statistics and analytics
        /// </summary>
        /// <param name="couponId">Coupon ID (optional - if not provided, returns overall statistics)</param>
        /// <returns>Coupon usage statistics</returns>
        [HttpGet("statistics")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetCouponStatistics([FromQuery] Guid? couponId = null)
        {
            try
            {
                var result = await _orderCouponService.GetWithIncludesAsync();

                if (!result.IsSuccess)
                {
                    return StatusCode(500, new
                    {
                        Success = false,
                        Message = result.ErrorMessage
                    });
                }

                var orderCoupons = result.Data?.ToList() ?? new List<OrderCouponDto>();

                if (couponId.HasValue)
                {
                    orderCoupons = orderCoupons.Where(oc => oc.CouponId == couponId.Value).ToList();
                }

                var statistics = new
                {
                    TotalApplications = orderCoupons.Count,
                    TotalDiscountAmount = orderCoupons.Sum(oc => oc.DiscountApplied),
                    AverageDiscountPerApplication = orderCoupons.Any()
                        ? orderCoupons.Average(oc => oc.DiscountApplied) : 0,
                    UniqueOrdersCount = orderCoupons.Select(oc => oc.OrderId).Distinct().Count(),
                    UniqueCouponsUsed = couponId.HasValue ? 1 : orderCoupons.Select(oc => oc.CouponId).Distinct().Count(),
                    CouponId = couponId,
                    TopCoupons = couponId.HasValue ? null : orderCoupons
                        .GroupBy(oc => oc.CouponId)
                        .Select(g => new
                        {
                            CouponId = g.Key,
                            UsageCount = g.Count(),
                            TotalDiscount = g.Sum(oc => oc.DiscountApplied),
                            CouponCode = g.FirstOrDefault()?.Coupon?.Code
                        })
                        .OrderByDescending(x => x.UsageCount)
                        .Take(5)
                        .ToList()
                };

                return Ok(new
                {
                    Success = true,
                    Message = "Statistics retrieved successfully",
                    Data = statistics
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving coupon statistics for coupon ID: {CouponId}", couponId);
                return StatusCode(500, new
                {
                    Success = false,
                    Message = "An error occurred while retrieving statistics",
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


