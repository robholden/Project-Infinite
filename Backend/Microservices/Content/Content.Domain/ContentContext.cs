using Microsoft.EntityFrameworkCore;

namespace Content.Domain;

public class ContentContext : DbContext
{
    public ContentContext(DbContextOptions<ContentContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
    }
}