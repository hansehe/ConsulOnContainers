using Microsoft.AspNetCore.Mvc;

namespace APIService.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class StatusController : ControllerBase
    {
        [HttpGet("")]
        [HttpHead("")]
        // Get Ok ping
        public IActionResult Ping()
        {
            return Ok();
        }

        // GET api/values/health
        [HttpGet("health")]
        public IActionResult GetHealth()
        {
            return Ok("I'm alive and healthy!");
        }
    }
}