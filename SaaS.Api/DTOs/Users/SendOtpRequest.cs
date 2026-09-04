using System.ComponentModel.DataAnnotations;

namespace SaaS.Api.DTOs.Users
{
    public class SendOtpRequest
    {
        [EmailAddress]
        [Required]
        public string Email { get; set; } = string.Empty;
    }
}
