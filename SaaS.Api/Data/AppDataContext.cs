using Microsoft.EntityFrameworkCore;
using SaaS.Api.Models;
using SaaS.Models;
using System.Dynamic;

namespace SaaS.Api.Data
{
    public class ApplicationDBContext : DbContext
    {
        public ApplicationDBContext(DbContextOptions<ApplicationDBContext> options) : base(options)
        {
        }
        public DbSet<User> Users => Set<User>();
        public DbSet<Template> Templates => Set<Template>();
        public DbSet<Plan> Plans => Set<Plan>();
        public DbSet<Report> Reports => Set<Report>();
        public DbSet<ReportJob> ReportJobs => Set<ReportJob>();
        public DbSet<AiUsageLog> AiUsageLogs => Set<AiUsageLog>();
        public DbSet<Subscription> Subscriptions => Set<Subscription>();
        public DbSet<OtpCode> OtpCodes => Set<OtpCode>();
        public DbSet<Role> Roles => Set<Role>();
        public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDBContext).Assembly);
        }
    }
}
