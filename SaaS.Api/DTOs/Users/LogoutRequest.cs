using System.ComponentModel.DataAnnotations;

namespace SaaS.Api.DTOs.Users
{
    public class LogoutRequest
    {
        [Required]
        public Guid UserId { get; set; }
    }
}
