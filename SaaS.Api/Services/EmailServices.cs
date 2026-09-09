using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using SaaS.Api.Models;
using SaaS.Api.Services.Interfaces;

namespace SaaS.Api.Services
{
    public class EmailServices : IEmailServices
    {
        private readonly EmailSettings _emailSettings;
        private readonly ILogger<EmailServices> _logger;

        public EmailServices(IOptions<EmailSettings> emailSettings, ILogger<EmailServices> logger)
        {
            _emailSettings = emailSettings.Value;
            _logger = logger;
        }

        public async Task SendOtpAsync(string recipientEmail, string otp, CancellationToken ct = default)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("SaaS Generation Report Web", _emailSettings.Email));
            message.To.Add(MailboxAddress.Parse(recipientEmail));
            message.Subject = "Mã OTP xác thực của bạn";

            message.Body = new TextPart("plain")
            {
                Text = $"""
                    SaaS Generation Report Web

                    Mã xác thực (OTP) của bạn là: {otp}

                    Mã này sẽ hết hạn sau 5 phút.
                    Nếu bạn không yêu cầu mã này, vui lòng bỏ qua email này.
                    """
            };

            using var smtp = new SmtpClient { Timeout = 10000 };

            try
            {
                await smtp.ConnectAsync(_emailSettings.SmtpServer, _emailSettings.Port, SecureSocketOptions.StartTls, ct);
                await smtp.AuthenticateAsync(_emailSettings.Email, _emailSettings.AppPassword, ct);
                await smtp.SendAsync(message, ct);
                await smtp.DisconnectAsync(true, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Gửi OTP thất bại cho email {Email}", recipientEmail);
                throw;
            }
        }
    }
}
