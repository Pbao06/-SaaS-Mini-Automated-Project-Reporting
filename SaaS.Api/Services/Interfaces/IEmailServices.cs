namespace SaaS.Api.Services.Interfaces
{
    public interface IEmailServices
    {
        Task SendOtpAsync(string recipientEmail, string otp,CancellationToken ct);
    }
}
