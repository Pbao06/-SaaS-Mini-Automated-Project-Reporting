using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using SaaS.Api.DTOs.Common;
using SaaS.Api.DTOs.Users;
using SaaS.Api.Services.Interfaces;

namespace SaaS.Api.Controllers
{
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthServices _authservices;
        public AuthController(IAuthServices authservices)
        {
            _authservices = authservices;
        }
        [HttpPost("/auth/register/otp")]
        public async Task<IActionResult> SendOtp([FromBody]SendOtpRequest request,CancellationToken ct)
        {
            var response = await _authservices.SendRegistrationOtpAsync(request,ct);
            return response.ToHTTPResponse();
        }
        [HttpPost("/auth/register/verify-otp")]
        public async Task<IActionResult> VerifyOtp([FromBody]VerifyOtpRequest request)
        {
            var response = await _authservices.VerifyRegistrationOtpAsync(request);
            return response.ToHTTPResponse();
        }
        [HttpPost("/auth/register")]
        public async Task<IActionResult> Register(RegisterRequest request)
        {
            var response = await _authservices.RegisterAsync(request);
            return response.ToHTTPResponse();
        }
    }
}
