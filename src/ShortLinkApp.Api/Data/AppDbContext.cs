using Microsoft.EntityFrameworkCore;

namespace ShortLinkApp.Api.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Link> Links => Set<Link>();
    public DbSet<ClickEvent> ClickEvents => Set<ClickEvent>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Link>(entity =>
        {
            entity.HasIndex(l => l.ShortCode).IsUnique();
            entity.HasIndex(l => l.CustomAlias).IsUnique().HasFilter("[CustomAlias] IS NOT NULL");
        });

        modelBuilder.Entity<ClickEvent>(entity =>
        {
            entity.HasOne(c => c.Link)
                  .WithMany(l => l.ClickEvents)
                  .HasForeignKey(c => c.LinkId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
