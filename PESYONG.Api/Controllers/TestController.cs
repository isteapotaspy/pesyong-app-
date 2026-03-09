using Microsoft.AspNetCore.Mvc;

namespace PESYONG.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TestController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get()
        {
            return Ok("PESYONG API is working");
        }
    }
}