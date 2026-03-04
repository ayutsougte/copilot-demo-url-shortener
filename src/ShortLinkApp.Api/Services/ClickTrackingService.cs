using Microsoft.EntityFrameworkCore;
using ShortLinkApp.Api.Data;

namespace ShortLinkApp.Api.Services;

/// <inheritdoc />
public class ClickTrackingService(AppDbContext dbContext) : IClickTrackingService
{
    /// <inheritdoc />
    public async Task RecordClickAsync(int linkId, string? referrer = null, CancellationToken cancellationToken = default)
    {
        dbContext.ClickEvents.Add(new ClickEvent
        {
            LinkId = linkId,
            ClickedAt = DateTime.UtcNow,
            Referrer = referrer
        });

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc />
    public Task<int> GetTotalClicksAsync(int linkId, CancellationToken cancellationToken = default)
    {
        return dbContext.ClickEvents
            .Where(c => c.LinkId == linkId)
            .CountAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<DailyClickCount>> GetClicksByDateAsync(int linkId, CancellationToken cancellationToken = default)
    {
        return await dbContext.ClickEvents
            .Where(c => c.LinkId == linkId)
            .GroupBy(c => new { c.ClickedAt.Year, c.ClickedAt.Month, c.ClickedAt.Day })
            .Select(g => new { g.Key.Year, g.Key.Month, g.Key.Day, Count = g.Count() })
            .OrderBy(r => r.Year).ThenBy(r => r.Month).ThenBy(r => r.Day)
            .AsAsyncEnumerable()
            .Select(r => new DailyClickCount(new DateOnly(r.Year, r.Month, r.Day), r.Count))
            .ToListAsync(cancellationToken);
    }
}
