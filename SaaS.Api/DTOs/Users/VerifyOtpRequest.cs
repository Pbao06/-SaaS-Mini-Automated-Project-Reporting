using System.ComponentModel.DataAnnotations;

namespace SaaS.Api.DTOs.Users
{
    public class VerifyOtpRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = null!;
        [Required]
        [RegularExpression(@"^\d{6}$", ErrorMessage = "OTP phải gồm đúng 6 chữ số")]
        public string Otp { get; set; } = null!;
    }
}
