using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SaaS.Api.Models;

namespace SaaS.Api.Data.Configurations
{
    public class AiUsageLogConfiguration : IEntityTypeConfiguration<AiUsageLog>
    {
        public void Configure(EntityTypeBuilder<AiUsageLog> builder)
        {
            builder.ToTable("AiUsageLogs");
            builder.HasKey(a => a.Id);

            builder.Property(x => x.Provider)
                   .HasMaxLength(50)
                   .IsRequired();
            builder.Property(x => x.InputTokens)
                   .IsRequired();

            builder.Property(x => x.OutputTokens)
                   .IsRequired();

            builder.Property(x => x.TotalTokens)
                   .IsRequired();

            builder.Property(x => x.EstimatedCost)
                   .HasPrecision(18, 8);

            builder.Property(x => x.Model)
                   .HasMaxLength(100)
                   .IsRequired();

            builder.HasIndex(x => new
            {
                x.ReportId,
                x.CreatedAt
            });
            builder.ToTable(t => t.HasCheckConstraint(
                "CK_AiUsageLogs_EstimatedCost_NonNegative",
                "EstimatedCost >= 0"));
            //Khoa ngoai Report 1-n AiUsageLog
            builder.HasOne(a => a.Report)
            .WithMany(r=> r.AiUsageLogs)
            .HasForeignKey(a => a.ReportId)
            .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
