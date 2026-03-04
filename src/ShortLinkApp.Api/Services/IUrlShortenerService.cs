using ShortLinkApp.Api.Data;

namespace ShortLinkApp.Api.Services;

public interface IUrlShortenerService
{
    /// <summary>
    /// Creates a new short link. If <paramref name="customAlias"/> is provided it is used as the
    /// short code; otherwise a random 6-character alphanumeric code is generated.
    /// Throws <see cref="InvalidOperationException"/> when the requested alias/code is already taken.
    /// </summary>
    Task<Link> CreateShortLinkAsync(string originalUrl, string? customAlias = null, DateTime? expiresAt = null);
}
