using System.ComponentModel.DataAnnotations;

namespace SaaS.Api.DTOs.Users
{
    public class RegisterAccountRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
        [Required]
        [MinLength(6)]
        public string Password { get; set; } = string.Empty;
        [Required]
        [MaxLength(50)]
        public string Fullname { get; set; } = string.Empty;
        [Required]
        public string VerifiedToken { get; set; } = string.Empty;
    }
}
