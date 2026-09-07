using SaaS.Api.Enum;
using SaaS.Api.Enums;

namespace SaaS.Api.Models
{
    public class OtpCode : BaseModel
    {
        public string Id { get; set; } = null!;
        public string Email { get; set; } = null!;
        public DateTime ExpiresAt { get; set; }
        public OtpCodePurpose Purpose { get; set;}
        public string CodeHash { get; set; } = null!;
        public DateTime? UsedAt { get; set; }
        public int Attempts { get; set; }
        public Guid? UserId { get; set; }
        public string? VerifiedToken { get; set; }
        public DateTime? VerifiedTokenDate { get; set; }
        public OtpCodeSendingStatus Status { get; set; }
        public User? User { get; set; }
    }
}
