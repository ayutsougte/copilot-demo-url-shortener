using ShortLinkApp.Api.Data;

namespace ShortLinkApp.Api.Services;

public interface ILinkRepository
{
    /// <summary>Returns <c>true</c> if any link row already uses <paramref name="code"/> as its
    /// <see cref="Link.ShortCode"/> or <see cref="Link.CustomAlias"/>.</summary>
    Task<bool> CodeOrAliasExistsAsync(string code, CancellationToken cancellationToken = default);

    /// <summary>Persists a new <see cref="Link"/> and returns it with the database-assigned id.</summary>
    Task<Link> AddLinkAsync(Link link, CancellationToken cancellationToken = default);
}
