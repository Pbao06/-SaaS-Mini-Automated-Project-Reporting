using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SaaS.Api.Models;

namespace SaaS.Api.Data.Configurations
{
    public class ReportJobConfiguration : IEntityTypeConfiguration<ReportJob>
    {
        public void Configure(EntityTypeBuilder<ReportJob> builder)
        {
            builder.ToTable("ReportJobs");

            builder.HasKey(r => r.Id);
            builder.Property(x => x.Status)
                   .HasConversion<string>()
                   .HasMaxLength(30)
                   .IsRequired();
            builder.Property(x => x.ErrorMessage)
            .HasMaxLength(2000);

            builder.HasIndex(x => new
            {
                x.ReportId,
                x.Status
            });
            //Khoa ngoai Report 1-n ReportJob
            builder.HasOne(r => r.Report)
                .WithMany(r=>r.ReportJobs)
                .HasForeignKey(r=>r.ReportId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
