namespace ShortLinkApp.Api.Endpoints;

/// <summary>
/// An <see cref="IEndpointFilter"/> that rejects requests whose <c>X-Api-Key</c> header does not
/// match the value configured under <c>ApiKey</c> in application settings.
/// Returns <c>401 Unauthorized</c> when the header is missing and <c>403 Forbidden</c> when the
/// value is present but incorrect.
/// </summary>
public sealed class ApiKeyEndpointFilter(IConfiguration configuration) : IEndpointFilter
{
    private const string HeaderName = "X-Api-Key";

    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var configuredKey = configuration["ApiKey"];

        if (string.IsNullOrWhiteSpace(configuredKey))
        {
            // No key configured — fail closed to protect the endpoint by default.
            return Results.StatusCode(StatusCodes.Status503ServiceUnavailable);
        }

        if (!context.HttpContext.Request.Headers.TryGetValue(HeaderName, out var providedKey))
            return Results.Unauthorized();

        if (!string.Equals(providedKey, configuredKey, StringComparison.Ordinal))
            // API key comparison is intentionally case-sensitive (Ordinal) for security.
            return Results.Forbid();

        return await next(context);
    }
}
