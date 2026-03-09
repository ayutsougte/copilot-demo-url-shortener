namespace ShortLinkApp.Api.Models;

/// <summary>
/// A single structured validation error that identifies the affected field and describes the problem.
/// </summary>
/// <param name="Field">The name of the field that failed validation (e.g. "Url", "CustomAlias", "ExpiresAt").</param>
/// <param name="Message">A human-readable error message suitable for display in the client UI.</param>
public sealed record ValidationError(string Field, string Message);
