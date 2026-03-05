namespace ShortLinkApp.Api.Endpoints;

/// <summary>
/// An <see cref="IEndpointFilter"/> that rejects requests whose <c>X-Api-Key</c> header does not
/// match the value configured under <c>ApiKey</c> in application settings.
/// When no key is configured the filter is a no-op and all requests are allowed through.
/// Returns <c>401 Unauthorized</c> when the header is missing and <c>403 Forbidden</c> when the
/// value is present but incorrect.
/// </summary>
public sealed class ApiKeyEndpointFilter(IConfiguration configuration, ILogger<ApiKeyEndpointFilter> logger) : IEndpointFilter
{
    private const string HeaderName = "X-Api-Key";

    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var configuredKey = configuration["ApiKey"];

        if (string.IsNullOrWhiteSpace(configuredKey))
        {
            // No key configured — endpoint is unprotected, allow all requests through.
            logger.LogWarning("No ApiKey configured. Endpoint '{Path}' is running without API key protection.",
                context.HttpContext.Request.Path);
            return await next(context);
        }

        if (!context.HttpContext.Request.Headers.TryGetValue(HeaderName, out var providedKey))
            return Results.Unauthorized();

        if (!string.Equals(providedKey, configuredKey, StringComparison.Ordinal))
            // API key comparison is intentionally case-sensitive (Ordinal) for security.
            return Results.Forbid();

        return await next(context);
    }
}
