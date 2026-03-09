namespace ShortLinkApp.Api.Models;

/// <summary>Aggregated statistics for the dashboard.</summary>
public sealed record DashboardStatsResponse(
    int TotalLinks,
    int TotalClicks,
    int ActiveLinks);
