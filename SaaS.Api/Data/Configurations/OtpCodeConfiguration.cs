using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SaaS.Api.Models;

namespace SaaS.Api.Data.Configurations;

public class OtpCodeConfiguration
    : IEntityTypeConfiguration<OtpCode>
{
    public void Configure(EntityTypeBuilder<OtpCode> builder)
    {
        builder.ToTable("OtpCodes");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Email)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(x => x.CodeHash)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(x => x.Purpose)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.ExpiresAt)
            .IsRequired();

        builder.Property(x => x.UsedAt);

        builder.Property(x => x.Attempts)
            .IsRequired();

        builder.HasIndex(x => new
        {
            x.Email,
            x.Purpose
        });
        //Khoa ngoai User 1-n Otpcode
        builder.HasOne(x => x.User)
            .WithMany(x => x.OtpCodes)
            .HasForeignKey(x=>x.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}