using ShortLinkApp.Api.Data;
using ShortLinkApp.Api.Models;
using ShortLinkApp.Api.Services;

namespace ShortLinkApp.Api.Endpoints;

public static class LinkManagementEndpoints
{
    public static void MapLinkManagementEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/links");

        group.MapGet("/", async (
                ILinkRepository linkRepository,
                CancellationToken cancellationToken) =>
            {
                var links = await linkRepository.GetAllLinksAsync(cancellationToken);
                return Results.Ok(links.Select(ToResponse));
            })
            .WithName("GetAllLinks");

        group.MapGet("/{id:int}", async (
                int id,
                ILinkRepository linkRepository,
                CancellationToken cancellationToken) =>
            {
                var link = await linkRepository.GetByIdAsync(id, cancellationToken);
                return link is null ? Results.NotFound() : Results.Ok(ToResponse(link));
            })
            .WithName("GetLinkById");

        group.MapPost("/", async (
                CreateLinkRequest request,
                IValidationService validationService,
                IUrlShortenerService urlShortenerService,
                CancellationToken cancellationToken) =>
            {
                var validation = validationService.ValidateCreateLinkRequest(
                    request.OriginalUrl,
                    request.CustomAlias,
                    request.ExpiresAt);

                if (!validation.IsValid)
                    return Results.ValidationProblem(
                        validation.Errors.ToDictionary(e => e.Field, e => new[] { e.Message }));

                try
                {
                    var link = await urlShortenerService.CreateShortLinkAsync(
                        request.OriginalUrl,
                        request.CustomAlias,
                        request.ExpiresAt,
                        cancellationToken);

                    return Results.Created($"/api/links/{link.Id}", ToResponse(link));
                }
                catch (InvalidOperationException ex)
                {
                    return Results.Conflict(new { error = ex.Message });
                }
            })
            .WithName("CreateLink")
            .AddEndpointFilter<ApiKeyEndpointFilter>();

        group.MapPut("/{id:int}", async (
                int id,
                UpdateLinkRequest request,
                ILinkRepository linkRepository,
                IValidationService validationService,
                TimeProvider timeProvider,
                CancellationToken cancellationToken) =>
            {
                var link = await linkRepository.GetByIdAsync(id, cancellationToken);
                if (link is null)
                    return Results.NotFound();

                // Validate any fields that are being changed.
                if (request.OriginalUrl is not null)
                {
                    var urlValidation = validationService.ValidateUrl(request.OriginalUrl);
                    if (!urlValidation.IsValid)
                        return Results.ValidationProblem(
                            urlValidation.Errors.ToDictionary(e => e.Field, e => new[] { e.Message }));
                }

                if (request.ExpiresAt is not null)
                {
                    var expiryValidation = validationService.ValidateExpirationDate(request.ExpiresAt);
                    if (!expiryValidation.IsValid)
                        return Results.ValidationProblem(
                            expiryValidation.Errors.ToDictionary(e => e.Field, e => new[] { e.Message }));
                }

                if (request.OriginalUrl is not null)
                    link.OriginalUrl = request.OriginalUrl;

                if (request.ExpiresAt is not null)
                    link.ExpiresAt = request.ExpiresAt;

                if (request.IsActive is not null)
                    link.IsActive = request.IsActive.Value;

                var updated = await linkRepository.UpdateLinkAsync(link, cancellationToken);
                return Results.Ok(ToResponse(updated));
            })
            .WithName("UpdateLink")
            .AddEndpointFilter<ApiKeyEndpointFilter>();

        group.MapDelete("/{id:int}", async (
                int id,
                ILinkRepository linkRepository,
                CancellationToken cancellationToken) =>
            {
                var deleted = await linkRepository.DeleteLinkAsync(id, cancellationToken);
                return deleted ? Results.NoContent() : Results.NotFound();
            })
            .WithName("DeleteLink")
            .AddEndpointFilter<ApiKeyEndpointFilter>();
    }

    private static LinkResponse ToResponse(Link link) =>
        new(link.Id, link.ShortCode, link.OriginalUrl, link.CustomAlias,
            link.CreatedAt, link.ExpiresAt, link.IsActive);
}
