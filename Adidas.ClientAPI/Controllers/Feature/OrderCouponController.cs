using Adidas.Application.Contracts.ServicesContracts.Feature;
using Adidas.DTOs.Feature.OrderCouponDTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Adidas.API.Controllers.Feature
{
    [ApiController]
    [Route("api/[controller]")]
    
    public class OrderCouponController : ControllerBase
    {
        private readonly IOrderCouponService _orderCouponService;
        private readonly ILogger<OrderCouponController> _logger;

        public OrderCouponController(
            IOrderCouponService orderCouponService,
            ILogger<OrderCouponController> logger)
        {
            _orderCouponService = orderCouponService;
            _logger = logger;
        }

        /// <summary>
        /// Get all order coupons with related data
        /// </summary>
        /// <returns>List of order coupons with coupon and order details</returns>
        [HttpGet]
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
        [HttpGet("{id:guid}")]
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
        [HttpGet("order/{orderId:guid}")]
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
        [HttpPost]
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
        [HttpPut("{id:guid}")]
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
        [HttpDelete("{id:guid}")]
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
}