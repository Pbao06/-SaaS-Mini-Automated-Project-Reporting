using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SaaS.Api.Controllers
{
    [ApiController]
    [Authorize]
    [Route("[controller]")]
    public class ReportController : ControllerBase
    {
        [HttpGet]
        public IActionResult Index()
        {
            return Ok("123");
        }
    }
}
