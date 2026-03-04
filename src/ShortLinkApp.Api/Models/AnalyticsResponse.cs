using ShortLinkApp.Api.Services;

namespace ShortLinkApp.Api.Models;

/// <summary>
/// Analytics data returned for a single short link.
/// </summary>
public sealed record AnalyticsResponse(
    int LinkId,
    int TotalClicks,
    IReadOnlyList<DailyClickCount> ClicksByDate);
