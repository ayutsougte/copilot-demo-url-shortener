namespace ShortLinkApp.Api.Services;

/// <summary>
/// Service for recording click events and aggregating click analytics.
/// </summary>
public interface IClickTrackingService
{
    /// <summary>
    /// Records a new click event for the specified link.
    /// </summary>
    /// <param name="linkId">The ID of the link that was clicked.</param>
    /// <param name="referrer">Optional HTTP referrer associated with the click.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task RecordClickAsync(int linkId, string? referrer = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns the total number of clicks recorded for the specified link.
    /// </summary>
    /// <param name="linkId">The ID of the link.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task<int> GetTotalClicksAsync(int linkId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns a time-series breakdown of clicks by date for the specified link.
    /// </summary>
    /// <param name="linkId">The ID of the link.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task<IReadOnlyList<DailyClickCount>> GetClicksByDateAsync(int linkId, CancellationToken cancellationToken = default);
}

/// <summary>
/// Represents the number of clicks recorded on a specific date.
/// </summary>
public record DailyClickCount(DateOnly Date, int Count);
