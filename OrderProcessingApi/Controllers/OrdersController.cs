using Microsoft.AspNetCore.Mvc;
using OrderProcessingApi.Models;
using OrderProcessingApi.Services;

namespace OrderProcessingApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrdersController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        [HttpPost]
        public async Task<IActionResult> PlaceOrder([FromBody] List<OrderItem> items)
        {
            try
            {
                var order = await _orderService.PlaceOrderAsync(items);
                return Ok(order);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("{id}/cancel")]
        public async Task<IActionResult> CancelOrder(Guid id)
        {
            try
            {
                await _orderService.CancelOrderAsync(id);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("{id}/status")]
        public async Task<IActionResult> GetOrderStatus(Guid id)
        {
            var order = await _orderService.GetOrderAsync(id);
            if (order == null)
                return NotFound(new { message = "Order not found." });

            return Ok(new { OrderId = order.Id, Status = order.Status.ToString() });
        }
    }
}
