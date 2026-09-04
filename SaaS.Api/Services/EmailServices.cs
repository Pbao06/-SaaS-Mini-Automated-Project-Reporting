using SaaS.Api.Services.Interfaces;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using Microsoft.Extensions.Options;

namespace SaaS.Api.Services
{
    public class EmailServices : IEmailServices
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailServices> _logger;
        public EmailServices(IConfiguration configuration, ILogger<EmailServices> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }
        public async Task SendOtpAsync(string recipientEmail, string otp, CancellationToken ct = default)
        {
            var email = _configuration["EmailSettings:Email"];
            var appPassword = _configuration["EmailSettings:AppPassword"];
            var smtpServer = _configuration["EmailSettings:SmtpServer"];
            var port = int.Parse(_configuration["EmailSettings:Port"]!);
            var message = new MimeMessage();
            message.From.Add(
                new MailboxAddress("SaaS Generation Report Web", email!)
            );
            message.To.Add(
                MailboxAddress.Parse(recipientEmail)
            );
            message.Subject = "Ma OTP: ";
            message.Body = new TextPart("plain")
            {
                Text = $"""
                    SaaS Generation Report Web

                    Your verification code is : {otp}

                    This code will expire in 5 minutes.

                    If you did not request this code, please ignore this email.
                    """
            };
            using var smtp = new SmtpClient() { Timeout = 10000};
            try
            {
                //Ket noi den Server cua Gmail Smtp Server bang giao thuc SMTP (Simple Mail Transfer Protocol)
                await smtp.ConnectAsync(
                        smtpServer!,
                        port,
                        SecureSocketOptions.StartTls,
                        ct
                    );
                //Xac thuc tai khoan mail
                await smtp.AuthenticateAsync(
                    email!,
                    appPassword!,
                    ct
                );
                //Gui mail
                await smtp.SendAsync(message,ct);
                //Ngat ket noi
                await smtp.DisconnectAsync(true,ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Gui OTP that bai cho email {recipientEmail}", recipientEmail);
                throw;
            }
        }
    }
}
