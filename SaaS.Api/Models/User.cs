using SaaS.Models;

namespace SaaS.Api.Models
{
    public class User : BaseModel
    {
        public Guid Id { get; set; }
        public string RoleId { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string PasswordHash { get; set; } = null!;
        public string FullName { get; set; } = null!;
        public bool IsActive { get; set; } = true;
        public ICollection<Report> Reports { get; set; } = new List<Report>();
        public ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();
        public ICollection<AiUsageLog> AiUsageLogs { get; set; } = new List<AiUsageLog>();
        public ICollection<OtpCode> OtpCodes {  get; set; } = new List<OtpCode>();
        public Role Role { get; set; } = new Role();
    }
}
