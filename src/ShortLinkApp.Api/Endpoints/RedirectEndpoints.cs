using ShortLinkApp.Api.Services;

namespace ShortLinkApp.Api.Endpoints;

public static class RedirectEndpoints
{
    public static void MapRedirectEndpoints(this WebApplication app)
    {
        var clientBaseUrl = app.Configuration["ClientBaseUrl"] ?? string.Empty;
        clientBaseUrl = clientBaseUrl.TrimEnd('/');

        app.MapGet("/{shortCode}", async (
                string shortCode,
                ILinkRepository linkRepository,
                IClickTrackingService clickTrackingService,
                TimeProvider timeProvider,
                HttpContext httpContext,
                CancellationToken cancellationToken) =>
            {
                var link = await linkRepository.GetByShortCodeAsync(shortCode, cancellationToken);

                if (link is null)
                    return Results.Redirect($"{clientBaseUrl}/link-not-found?code={Uri.EscapeDataString(shortCode)}", permanent: false);

                if (!link.IsActive || link.IsExpired(timeProvider.GetUtcNow().UtcDateTime))
                    return Results.Redirect($"{clientBaseUrl}/link-expired?code={Uri.EscapeDataString(shortCode)}", permanent: false);

                var referrer = httpContext.Request.Headers.Referer.ToString();
                await clickTrackingService.RecordClickAsync(link.Id, string.IsNullOrEmpty(referrer) ? null : referrer, cancellationToken);

                return Results.Redirect(link.OriginalUrl, permanent: false);
            })
            .WithName("RedirectShortLink");
    }
}
