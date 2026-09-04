using Microsoft.AspNetCore.Identity.Data;
using SaaS.Api.DTOs.Common;
using SaaS.Api.DTOs.Users;

namespace SaaS.Api.Services.Interfaces
{
    public interface IAuthServices
    {
        Task<ServicesResponse> SendRegistrationOtpAsync(SendOtpRequest request,CancellationToken ct);
        Task<ServicesResponse> VerifyRegistrationOtpAsync(VerifyOtpRequest request);
        Task<ServicesResponse> RegisterAsync(RegisterAccountRequest request);
        Task Logout();
        Task Login();
    }
}
