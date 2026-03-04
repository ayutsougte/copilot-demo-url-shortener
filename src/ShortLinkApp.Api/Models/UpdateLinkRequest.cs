namespace ShortLinkApp.Api.Models;

/// <summary>
/// Payload for <c>PUT /api/links/{id}</c>.
/// All fields are optional; only non-null values are applied.
/// </summary>
public sealed record UpdateLinkRequest(
    string? OriginalUrl,
    DateTime? ExpiresAt,
    bool? IsActive);
