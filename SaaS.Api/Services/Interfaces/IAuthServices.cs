using SaaS.Api.DTOs.Common;
using SaaS.Api.DTOs.Users;

namespace SaaS.Api.Services.Interfaces
{
    public interface IAuthServices
    {
        Task<ServicesResponse> SendRegistrationOtpAsync(SendOtpRequest request, CancellationToken ct);
        Task<ServicesResponse<VerifyOtpResponse>> VerifyRegistrationOtpAsync(VerifyOtpRequest request, CancellationToken ct);
        Task<ServicesResponse> RegisterAsync(RegisterAccountRequest request, CancellationToken ct);
        Task<ServicesResponse<LoginResponse>> LoginAsync(LoginRequest request, CancellationToken ct);
        Task<ServicesResponse> LogoutAsync(Guid userId, CancellationToken ct);
    }
}
