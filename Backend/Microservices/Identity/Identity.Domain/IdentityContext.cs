using Microsoft.EntityFrameworkCore;

namespace Identity.Domain;

public class IdentityContext : DbContext
{
    public IdentityContext(DbContextOptions<IdentityContext> options)
        : base(options)
    {
    }

    public DbSet<AuthToken> AuthTokens { get; set; }

    public DbSet<Password> Passwords { get; set; }

    public DbSet<RecoveryCode> RecoveryCodes { get; set; }

    public DbSet<TwoFactor> TwoFactors { get; set; }

    public DbSet<UserKey> UserKeys { get; set; }

    public DbSet<User> Users { get; set; }

    public DbSet<FailedLogin> FailedLogins { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Indexes
        modelBuilder.Entity<User>().HasIndex(x => x.Email).IsUnique();
        modelBuilder.Entity<User>().HasIndex(x => x.Username).IsUnique();
        modelBuilder.Entity<UserKey>().HasIndex(x => x.Key).IsUnique();

        // User Keys
        modelBuilder.Entity<UserKey>()
            .HasOne(x => x.User)
            .WithMany(x => x.UserKeys)
            .IsRequired()
            .HasForeignKey(x => x.UserId);

        // Password
        modelBuilder.Entity<Password>()
            .HasOne(x => x.User)
            .WithMany(x => x.Passwords)
            .IsRequired()
            .HasForeignKey(x => x.UserId);

        // Auth Tokens
        modelBuilder.Entity<AuthToken>()
            .HasOne(x => x.User)
            .WithMany(x => x.AuthTokens)
            .HasForeignKey(x => x.UserId);

        // Recovery Codes
        modelBuilder.Entity<RecoveryCode>()
            .HasOne(x => x.User)
            .WithMany(x => x.RecoveryCodes)
            .IsRequired()
            .HasForeignKey(x => x.UserId);
    }
}