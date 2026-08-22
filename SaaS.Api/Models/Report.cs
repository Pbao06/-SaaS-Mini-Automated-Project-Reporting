using SaaS.Api.Models;
using SaaS.Api.Enum;
namespace SaaS.Models;

public class Report : BaseModel
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }
    public Guid? TemplateId { get; set; }

    public string Title { get; set; } = null!;

    public string FileName { get; set; } = null!;

    public string FilePath { get; set; } = null!;

    public ReportStatus Status { get; set; } = ReportStatus.Draft;

    public User User { get; set; } = null!;
    public Template? Template { get; set; }

    public ICollection<ReportJob> ReportJobs { get; set; } = new List<ReportJob>();
    public ICollection<AiUsageLog> AiUsageLogs { get; set; } = new List<AiUsageLog>();
}