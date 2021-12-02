using Microsoft.EntityFrameworkCore;

namespace Content.Domain;

public class ContentContext : DbContext
{
    public ContentContext(DbContextOptions<ContentContext> options)
        : base(options)
    {
    }

    public DbSet<UserSetting> UserSettings { get; set; }

    public DbSet<Collection> Collections { get; set; }

    public DbSet<Country> Countries { get; set; }

    public DbSet<Location> Locations { get; set; }

    public DbSet<Boundry> Boundries { get; set; }

    public DbSet<Picture> Pictures { get; set; }

    public DbSet<Tag> Tags { get; set; }

    public DbSet<PictureLike> PictureLikes { get; set; }

    public DbSet<PictureLocationRequest> PictureLocationRequests { get; set; }

    public DbSet<PictureModeration> PictureModerations { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Location>().Property(x => x.Lat).HasPrecision(18, 6);
        modelBuilder.Entity<Location>().Property(x => x.Lng).HasPrecision(18, 6);

        modelBuilder.Entity<Picture>().Property(x => x.Lat).HasPrecision(18, 6);
        modelBuilder.Entity<Picture>().Property(x => x.Lng).HasPrecision(18, 6);

        modelBuilder.Entity<Country>().Property(x => x.Lat).HasPrecision(18, 6);
        modelBuilder.Entity<Country>().Property(x => x.Lng).HasPrecision(18, 6);

        modelBuilder.Entity<Boundry>().Property(x => x.MinLat).HasPrecision(18, 6);
        modelBuilder.Entity<Boundry>().Property(x => x.MaxLat).HasPrecision(18, 6);
        modelBuilder.Entity<Boundry>().Property(x => x.MinLng).HasPrecision(18, 6);
        modelBuilder.Entity<Boundry>().Property(x => x.MaxLng).HasPrecision(18, 6);

        modelBuilder.Entity<PictureLike>().HasKey(t => new { t.PictureLikeId, t.UserId, t.PictureId });

        modelBuilder.Entity<Tag>().HasIndex(x => x.Value).IsUnique();

        modelBuilder.Entity<PictureLocationRequest>().Property(p => p.RowVersion).IsConcurrencyToken();

        modelBuilder.Entity<PictureModeration>().HasKey(t => new { t.ModerationId, t.PictureId });
        modelBuilder.Entity<PictureModeration>().Property(p => p.RowVersion).IsConcurrencyToken();
    }
}