namespace ShortLinkApp.Api.Data;

public class Link
{
    public int Id { get; set; }
    public string ShortCode { get; set; } = string.Empty;
    public string OriginalUrl { get; set; } = string.Empty;
    public string? CustomAlias { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ExpiresAt { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<ClickEvent> ClickEvents { get; set; } = [];

    /// <summary>
    /// Returns <c>true</c> if this link has passed its expiration date as of the given UTC time.
    /// </summary>
    public bool IsExpired(DateTime utcNow) => ExpiresAt.HasValue && ExpiresAt.Value <= utcNow;
}
