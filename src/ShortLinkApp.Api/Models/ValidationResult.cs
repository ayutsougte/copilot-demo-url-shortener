namespace ShortLinkApp.Api.Models;

/// <summary>
/// Represents the result of a validation operation, carrying a list of structured error messages
/// suitable for returning to the client.
/// </summary>
public sealed class ValidationResult
{
    private ValidationResult(bool isValid, IReadOnlyList<ValidationError> errors)
    {
        IsValid = isValid;
        Errors = errors;
    }

    /// <summary>Gets a value indicating whether validation passed with no errors.</summary>
    public bool IsValid { get; }

    /// <summary>Gets the (possibly empty) list of validation errors.</summary>
    public IReadOnlyList<ValidationError> Errors { get; }

    /// <summary>Returns a successful validation result.</summary>
    public static ValidationResult Success() => new(true, []);

    /// <summary>Returns a failed validation result with the supplied errors.</summary>
    public static ValidationResult Failure(IEnumerable<ValidationError> errors) =>
        new(false, errors.ToList().AsReadOnly());

    /// <summary>Returns a failed validation result with a single error.</summary>
    public static ValidationResult Failure(string field, string message) =>
        Failure([new ValidationError(field, message)]);
}
