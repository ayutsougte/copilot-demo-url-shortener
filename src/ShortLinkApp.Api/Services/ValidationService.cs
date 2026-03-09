using System.Text.RegularExpressions;
using ShortLinkApp.Api.Models;

namespace ShortLinkApp.Api.Services;

/// <summary>
/// Default implementation of <see cref="IValidationService"/>.
/// </summary>
public partial class ValidationService(TimeProvider timeProvider) : IValidationService
{
    // Aliases may only contain letters, digits, hyphens, and underscores.
    [GeneratedRegex(@"^[a-zA-Z0-9_-]+$")]
    private static partial Regex AliasPattern();

    /// <summary>
    /// Path segments that are reserved by the application and must not be used as custom aliases.
    /// </summary>
    private static readonly HashSet<string> ReservedPaths =
        new(StringComparer.OrdinalIgnoreCase)
        {
            "api",
            "admin",
            "login",
            "logout",
            "register",
            "health",
            "swagger",
            "openapi",
        };

    /// <inheritdoc/>
    public ValidationResult ValidateUrl(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
            return ValidationResult.Failure("Url", "URL is required.");

        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri) ||
            (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps) ||
            !uri.IsWellFormedOriginalString() ||
            string.IsNullOrEmpty(uri.Host) ||
            !string.IsNullOrEmpty(uri.UserInfo))
        {
            return ValidationResult.Failure("Url",
                "URL must be a well-formed absolute URL with an http or https scheme.");
        }

        return ValidationResult.Success();
    }

    /// <inheritdoc/>
    public ValidationResult ValidateAlias(string alias)
    {
        if (string.IsNullOrWhiteSpace(alias))
            return ValidationResult.Failure("CustomAlias", "Custom alias must not be empty.");

        if (!AliasPattern().IsMatch(alias))
            return ValidationResult.Failure("CustomAlias",
                "Custom alias may only contain letters, digits, hyphens (-), and underscores (_).");

        if (ReservedPaths.Contains(alias))
            return ValidationResult.Failure("CustomAlias",
                $"The alias '{alias}' conflicts with a reserved application path.");

        return ValidationResult.Success();
    }

    /// <inheritdoc/>
    public ValidationResult ValidateExpirationDate(DateTime? expiresAt)
    {
        if (expiresAt is null)
            return ValidationResult.Success();

        var expiresAtUtc = expiresAt.Value;

        if (expiresAtUtc.Kind != DateTimeKind.Utc)
            return ValidationResult.Failure("ExpiresAt",
                "Expiration date must be specified in UTC.");

        if (expiresAtUtc <= timeProvider.GetUtcNow().UtcDateTime)
            return ValidationResult.Failure("ExpiresAt",
                "Expiration date must be a date and time in the future.");

        return ValidationResult.Success();
    }

    /// <inheritdoc/>
    public ValidationResult ValidateCreateLinkRequest(string url, string? customAlias, DateTime? expiresAt)
    {
        var errors = new List<ValidationError>();

        var urlResult = ValidateUrl(url);
        if (!urlResult.IsValid)
            errors.AddRange(urlResult.Errors);

        if (customAlias is not null)
        {
            var aliasResult = ValidateAlias(customAlias);
            if (!aliasResult.IsValid)
                errors.AddRange(aliasResult.Errors);
        }

        var expiryResult = ValidateExpirationDate(expiresAt);
        if (!expiryResult.IsValid)
            errors.AddRange(expiryResult.Errors);

        return errors.Count == 0 ? ValidationResult.Success() : ValidationResult.Failure(errors);
    }
}
