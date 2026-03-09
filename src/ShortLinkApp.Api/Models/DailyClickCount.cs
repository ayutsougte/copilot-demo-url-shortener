namespace ShortLinkApp.Api.Models;

/// <summary>
/// Represents the number of clicks recorded on a specific date.
/// </summary>
public record DailyClickCount(DateOnly Date, int Count);
