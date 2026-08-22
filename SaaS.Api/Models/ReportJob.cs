using SaaS.Api.Enum;
using SaaS.Models;

namespace SaaS.Api.Models
{
    public class ReportJob
    {
        public Guid Id { get; set; }

        public Guid ReportId { get; set; }

        public ReportJobStatus Status { get; set; } = ReportJobStatus.Pending;

        public string? ErrorMessage { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }

        public Report Report { get; set; } = null!;
    }
}
