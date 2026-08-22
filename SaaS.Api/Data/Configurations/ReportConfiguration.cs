using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SaaS.Models;

namespace SaaS.Api.Data.Configurations
{
    public class ReportConfiguration : IEntityTypeConfiguration<Report>
    {
        public void Configure(EntityTypeBuilder<Report> builder)
        {
            builder.ToTable("Reports");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Title)
                .HasMaxLength(300)
                .IsRequired();

            builder.Property(x => x.FileName)
                .HasMaxLength(500);

            builder.Property(x => x.FilePath)
                .HasMaxLength(1000);

            builder.Property(x => x.Status)
                .HasConversion<string>()
                .HasMaxLength(30)
                .IsRequired();

            builder.HasIndex(x => x.UserId);
            builder.HasIndex(x => x.TemplateId);

            builder.HasOne(x => x.User)
                .WithMany(x => x.Reports)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            //Khoa ngoai User 1->N Report
            builder.HasOne(r => r.User)
                .WithMany(u => u.Reports)
                .HasForeignKey(r=>r.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            //Khoa ngoai Template 1->N Report
            builder.HasOne(r => r.Template)
                .WithMany(u => u.Reports)
                .HasForeignKey(r => r.TemplateId)
                .OnDelete(DeleteBehavior.SetNull);

        }
    }
}
