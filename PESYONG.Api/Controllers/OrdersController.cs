using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using PESYONG.Api.Hubs;
using PESYONG.ApplicationLogic.DTOs;
using PESYONG.ApplicationLogic.Repositories;
using PESYONG.Domain.Enums;

namespace PESYONG.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrdersController : ControllerBase
    {
        private readonly OrderRepository _orderRepository;
        private readonly IHubContext<PesyongHub> _hubContext;

        public OrdersController(
            OrderRepository orderRepository,
            IHubContext<PesyongHub> hubContext)
        {
            _orderRepository = orderRepository;
            _hubContext = hubContext;
        }

        [HttpPut("{orderId}/status")]
        public async Task<IActionResult> UpdateStatus(Guid orderId, [FromBody] UpdateOrderStatusDto dto)
        {
            if (!Enum.TryParse<DeliveryStatus>(dto.Status, true, out var newStatus))
            {
                return BadRequest(new
                {
                    Message = $"Invalid status: {dto.Status}"
                });
            }

            var order = await _orderRepository.GetOrderByIdAsync(orderId);
            if (order == null)
            {
                return NotFound(new
                {
                    Message = $"Order {orderId} was not found."
                });
            }

            await _orderRepository.UpdateOrderStatusAsync(orderId, newStatus);

            await _hubContext.Clients.All.SendAsync(
                "OrderStatusChanged",
                orderId.ToString(),
                newStatus.ToString());

            return Ok(new
            {
                OrderId = orderId,
                Status = newStatus.ToString()
            });
        }
    }
}