using Microsoft.EntityFrameworkCore;
using ShortLinkApp.Api.Data;

namespace ShortLinkApp.Api.Services;

public class LinkRepository(AppDbContext dbContext) : ILinkRepository
{
    public Task<bool> CodeExistsAsync(string code) =>
        dbContext.Links.AnyAsync(l => l.ShortCode == code || l.CustomAlias == code);

    public async Task<Link> AddLinkAsync(Link link)
    {
        dbContext.Links.Add(link);
        await dbContext.SaveChangesAsync();
        return link;
    }
}
