using Microsoft.AspNetCore.Mvc;
using SaaS.Api.Services.Interfaces;

namespace SaaS.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AIController : ControllerBase
    {
        private readonly IAIServices services;
        public AIController(IAIServices services)
        {
            this.services = services;
        }
        [HttpGet()]
        public async Task<IActionResult> Index()
        {
            await services.Test(content:"iphone cua hang nao ?");
            return Ok("123");
        }
    }
}
