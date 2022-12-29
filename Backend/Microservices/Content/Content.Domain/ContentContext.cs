using Microsoft.EntityFrameworkCore;

namespace Content.Domain;

public class ContentContext : DbContext
{
    public ContentContext(DbContextOptions<ContentContext> options)
        : base(options)
    {
    }

    public DbSet<Post> Posts { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
    }
}