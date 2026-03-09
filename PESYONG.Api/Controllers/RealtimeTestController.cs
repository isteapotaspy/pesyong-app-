using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using PESYONG.Api.Hubs;

namespace PESYONG.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RealtimeTestController : ControllerBase
    {
        private readonly IHubContext<PesyongHub> _hubContext;

        public RealtimeTestController(IHubContext<PesyongHub> hubContext)
        {
            _hubContext = hubContext;
        }

        [HttpPost("catalog-changed")]
        public async Task<IActionResult> CatalogChanged()
        {
            await _hubContext.Clients.All.SendAsync("CatalogChanged");
            return Ok("CatalogChanged event sent.");
        }
    }
}