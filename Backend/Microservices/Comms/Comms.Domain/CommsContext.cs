using Microsoft.EntityFrameworkCore;

namespace Comms.Domain;

public class CommsContext : DbContext
{
    public CommsContext(DbContextOptions<CommsContext> options)
        : base(options)
    {
    }

    public DbSet<Notification> Notifications { get; set; }

    public DbSet<NotificationEntry> NotificationEntries { get; set; }

    public DbSet<EmailQueue> EmailQueue { get; set; }

    public DbSet<Email> Emails { get; set; }

    public DbSet<Sms> Sms { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<EmailQueue>()
            .Property(x => x.RowVersion)
            .IsConcurrencyToken();
    }
}