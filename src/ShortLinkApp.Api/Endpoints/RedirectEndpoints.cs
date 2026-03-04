using ShortLinkApp.Api.Services;

namespace ShortLinkApp.Api.Endpoints;

public static class RedirectEndpoints
{
    public static void MapRedirectEndpoints(this WebApplication app)
    {
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
                    return Results.NotFound();

                if (!link.IsActive || link.IsExpired(timeProvider.GetUtcNow().UtcDateTime))
                    return Results.StatusCode(StatusCodes.Status410Gone);

                var referrer = httpContext.Request.Headers.Referer.ToString();
                await clickTrackingService.RecordClickAsync(link.Id, string.IsNullOrEmpty(referrer) ? null : referrer, cancellationToken);

                return Results.Redirect(link.OriginalUrl, permanent: false);
            })
            .WithName("RedirectShortLink");
    }
}
