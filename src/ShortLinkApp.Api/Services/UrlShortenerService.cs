using System.Security.Cryptography;
using ShortLinkApp.Api.Data;

namespace ShortLinkApp.Api.Services;

public class UrlShortenerService(ILinkRepository linkRepository) : IUrlShortenerService
{
    private const int ShortCodeLength = 6;
    private const string AlphanumericChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";

    public async Task<Link> CreateShortLinkAsync(string originalUrl, string? customAlias = null, DateTime? expiresAt = null, CancellationToken cancellationToken = default)
    {
        string shortCode;

        if (customAlias is not null)
        {
            if (await linkRepository.CodeOrAliasExistsAsync(customAlias, cancellationToken))
                throw new InvalidOperationException($"The alias '{customAlias}' is already in use.");

            // ShortCode is the value used for lookups/redirection; CustomAlias records that
            // this code was user-supplied rather than auto-generated.
            shortCode = customAlias;
        }
        else
        {
            shortCode = await GenerateUniqueShortCodeAsync(cancellationToken);
        }

        var link = new Link
        {
            ShortCode = shortCode,
            CustomAlias = customAlias,
            OriginalUrl = originalUrl,
            ExpiresAt = expiresAt
        };

        return await linkRepository.AddLinkAsync(link, cancellationToken);
    }

    private async Task<string> GenerateUniqueShortCodeAsync(CancellationToken cancellationToken = default)
    {
        const int maxAttempts = 10;

        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            string code = RandomNumberGenerator.GetString(AlphanumericChars, ShortCodeLength);

            if (!await linkRepository.CodeOrAliasExistsAsync(code, cancellationToken))
                return code;
        }

        throw new InvalidOperationException("Unable to generate a unique short code after multiple attempts.");
    }
}
