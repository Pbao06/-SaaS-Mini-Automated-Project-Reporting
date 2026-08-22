using SaaS.Models;

namespace SaaS.Api.Models
{
    public class AiUsageLog
    {
        public Guid Id { get; set; }

        public Guid? ReportId { get; set; }

        public string Provider { get; set; } = null!;
        public string Model { get; set; } = null!;

        public long InputTokens { get; set; }
        public long OutputTokens { get; set; }

        public long TotalTokens { get; set; }

        public decimal? EstimatedCost { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public Report Report { get; set; } = null!;
    }
}
