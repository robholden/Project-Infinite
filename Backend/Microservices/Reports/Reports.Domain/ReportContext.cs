using Microsoft.EntityFrameworkCore;

namespace Reports.Domain;

public class ReportContext : DbContext
{
    public ReportContext(DbContextOptions<ReportContext> options)
        : base(options)
    {
    }

    public DbSet<ReportAction> Actions { get; set; }

    public DbSet<UserReport> UserReports { get; set; }
    public DbSet<UserReportInstance> UserReportInstances { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserReportInstance>()
            .HasOne(x => x.UserReport)
            .WithMany(x => x.Reports)
            .HasForeignKey(x => x.ReportId)
            .IsRequired();

        modelBuilder.Entity<UserReport>()
           .HasOne(x => x.Action)
           .WithMany()
           .HasForeignKey(x => x.ActionId)
           .IsRequired(false);
    }
}