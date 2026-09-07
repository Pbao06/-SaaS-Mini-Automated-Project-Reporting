using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
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
        [EnableRateLimiting("auth-login")]
        public async Task<IActionResult> LoginAccount([FromBody] LoginRequest request, CancellationToken ct)
        {
            var response = await _authservices.LoginAsync(request, ct);
            return response.ToHTTPResponse();
        }

        [HttpPost("logout")]
        public async Task<IActionResult> LogoutAccount([FromBody] LogoutRequest request, CancellationToken ct)
        {
            var response = await _authservices.LogoutAsync(request.UserId, ct);
            return response.ToHTTPResponse();
        }

        [HttpPost("register/otp")]
        [EnableRateLimiting("auth-otp")]
        public async Task<IActionResult> SendOtp([FromBody] SendOtpRequest request, CancellationToken ct)
        {
            var response = await _authservices.SendRegistrationOtpAsync(request, ct);
            return response.ToHTTPResponse();
        }

        [HttpPost("register/verify-otp")]
        [EnableRateLimiting("auth-otp")]
        public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpRequest request, CancellationToken ct)
        {
            var response = await _authservices.VerifyRegistrationOtpAsync(request, ct);
            return response.ToHTTPResponse();
        }

        [HttpPost("register/create")]
        public async Task<IActionResult> Register([FromBody] RegisterAccountRequest request, CancellationToken ct)
        {
            var response = await _authservices.RegisterAsync(request, ct);
            return response.ToHTTPResponse();
        }
    }
}
