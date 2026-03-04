namespace ShortLinkApp.Api.Models;

/// <summary>
/// Payload for <c>POST /api/links</c>.
/// </summary>
public sealed record CreateLinkRequest(
    string OriginalUrl,
    string? CustomAlias,
    DateTime? ExpiresAt);
