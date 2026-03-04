using Microsoft.EntityFrameworkCore;
using ShortLinkApp.Api.Data;

namespace ShortLinkApp.Api.Services;

public class LinkRepository(AppDbContext dbContext) : ILinkRepository
{
    public Task<bool> CodeOrAliasExistsAsync(string code, CancellationToken cancellationToken = default) =>
        dbContext.Links.AnyAsync(l => l.ShortCode == code || l.CustomAlias == code, cancellationToken);

    public async Task<Link> AddLinkAsync(Link link, CancellationToken cancellationToken = default)
    {
        dbContext.Links.Add(link);
        await dbContext.SaveChangesAsync(cancellationToken);
        return link;
    }

    public Task<Link?> GetByShortCodeAsync(string shortCode, CancellationToken cancellationToken = default) =>
        dbContext.Links.FirstOrDefaultAsync(
            l => l.ShortCode == shortCode || l.CustomAlias == shortCode,
            cancellationToken);

    public Task<List<Link>> GetAllLinksAsync(CancellationToken cancellationToken = default) =>
        dbContext.Links
            .OrderByDescending(l => l.CreatedAt)
            .ToListAsync(cancellationToken);

    public Task<Link?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        dbContext.Links.FindAsync([id], cancellationToken).AsTask();

    public async Task<Link> UpdateLinkAsync(Link link, CancellationToken cancellationToken = default)
    {
        // The entity is already tracked by EF Core (fetched in the same scoped DbContext),
        // so the change tracker will detect modified properties automatically.
        await dbContext.SaveChangesAsync(cancellationToken);
        return link;
    }

    public async Task<bool> DeleteLinkAsync(int id, CancellationToken cancellationToken = default)
    {
        var link = await dbContext.Links.FindAsync([id], cancellationToken);
        if (link is null)
            return false;

        dbContext.Links.Remove(link);
        await dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }
}
