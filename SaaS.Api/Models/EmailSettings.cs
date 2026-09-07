namespace SaaS.Api.Models
{
    public class EmailSettings
    {
        public string Email { get; set; } = string.Empty;
        public string AppPassword { get; set; } = string.Empty;
        public string SmtpServer { get; set; } = string.Empty;
        public int Port { get; set; }
    }
}
