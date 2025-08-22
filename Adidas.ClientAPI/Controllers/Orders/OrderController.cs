using Adidas.Application.Contracts.ServicesContracts.Operation;
using Adidas.Application.Services.Operation;
using Adidas.Models.Operation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Adidas.ClientAPI.Controllers.Operation
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService orderService;

        public OrderController(IOrderService orderService)
        {
            this.orderService = orderService;
        }
        [HttpGet("GetAllOrdersByUserId")]
        public async Task<IActionResult> GetAllOrdersByUserId()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier); 
            var orders = await orderService.GetOrdersByUserIdAsync(userId);
            return Ok(orders);
        }
        [HttpGet("GetOrderByUserId/{userId}")]
        public async Task<IActionResult> GetOrderByUserId(string userId)
        {
            var result = await orderService.GetOrderByUserIdAsync(userId);

            if (!result.IsSuccess)
                return NotFound(new { Message = result.ErrorMessage });

            return Ok(result.Data);
        }

        [HttpGet("active")]
        public async Task<IActionResult> GetActiveOrder()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var result = await orderService.GetOrdersByUserIdAsync(userId);
            if (!result.IsSuccess) return BadRequest(result.ErrorMessage);

            var activeOrder = result.Data.FirstOrDefault(o => o.OrderStatus == OrderStatus.Pending);
            if (activeOrder == null) return NotFound("No active order found");

            return Ok(activeOrder);
        }


    }
}
