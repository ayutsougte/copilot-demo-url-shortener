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
}
