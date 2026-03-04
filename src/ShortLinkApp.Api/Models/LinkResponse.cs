namespace ShortLinkApp.Api.Models;

/// <summary>
/// Representation of a short link returned by the management API.
/// </summary>
public sealed record LinkResponse(
    int Id,
    string ShortCode,
    string OriginalUrl,
    string? CustomAlias,
    DateTime CreatedAt,
    DateTime? ExpiresAt,
    bool IsActive);
