using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using SaaS.Api.DTOs.Common;
using SaaS.Api.DTOs.Users;
using SaaS.Api.Services.Interfaces;

namespace SaaS.Api.Controllers
{
    [ApiController]
    [Route("auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthServices _authservices;
        public AuthController(IAuthServices authservices)
        {
            _authservices = authservices;
        }
        [HttpPost("login")]
        public async Task<IActionResult> LoginAccount()
        {
            throw new Exception();
        }

        [HttpPost("register/otp")]
        public async Task<IActionResult> SendOtp([FromBody]SendOtpRequest request,CancellationToken ct)
        {
            var response = await _authservices.SendRegistrationOtpAsync(request,ct);
            return response.ToHTTPResponse();
        }
        [HttpPost("register/verify-otp")]
        public async Task<IActionResult> VerifyOtp([FromBody]VerifyOtpRequest request,CancellationToken ct)
        {
            var response = await _authservices.VerifyRegistrationOtpAsync(request,ct);
            return response.ToHTTPResponse();
        }
        [HttpPost("register/create")]
        public async Task<IActionResult> Register([FromBody]RegisterAccountRequest request,CancellationToken ct)
        {
            var response = await _authservices.RegisterAsync(request,ct);
            return response.ToHTTPResponse();
        }
    }
}
