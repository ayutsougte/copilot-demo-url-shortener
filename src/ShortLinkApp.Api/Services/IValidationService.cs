namespace ShortLinkApp.Api.Services;

/// <summary>
/// Provides validation for URLs, custom aliases, and expiration dates submitted when creating or
/// updating a short link.
/// </summary>
public interface IValidationService
{
    /// <summary>
    /// Validates that <paramref name="url"/> is a well-formed absolute URL using the
    /// <c>http</c> or <c>https</c> scheme.
    /// </summary>
    /// <param name="url">The URL to validate.</param>
    /// <returns>A <see cref="ValidationResult"/> indicating success or containing error details.</returns>
    ValidationResult ValidateUrl(string url);

    /// <summary>
    /// Validates that <paramref name="alias"/> contains only URL-safe characters and does not
    /// conflict with paths reserved by the application.
    /// </summary>
    /// <param name="alias">The custom alias to validate.</param>
    /// <returns>A <see cref="ValidationResult"/> indicating success or containing error details.</returns>
    ValidationResult ValidateAlias(string alias);

    /// <summary>
    /// Validates that <paramref name="expiresAt"/>, when provided, is a date/time in the future.
    /// </summary>
    /// <param name="expiresAt">The optional expiration date to validate.</param>
    /// <returns>A <see cref="ValidationResult"/> indicating success or containing error details.</returns>
    ValidationResult ValidateExpirationDate(DateTime? expiresAt);

    /// <summary>
    /// Runs all validations (<see cref="ValidateUrl"/>, <see cref="ValidateAlias"/> when an alias
    /// is provided, and <see cref="ValidateExpirationDate"/> when an expiration date is provided)
    /// and returns a combined result.
    /// </summary>
    /// <param name="url">The URL to validate.</param>
    /// <param name="customAlias">The optional custom alias to validate.</param>
    /// <param name="expiresAt">The optional expiration date to validate.</param>
    /// <returns>A <see cref="ValidationResult"/> that aggregates all errors found.</returns>
    ValidationResult ValidateCreateLinkRequest(string url, string? customAlias, DateTime? expiresAt);
}
