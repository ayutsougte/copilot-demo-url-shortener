using ShortLinkApp.Api.Data;

namespace ShortLinkApp.Api.Services;

public interface ILinkRepository
{
    /// <summary>Returns <c>true</c> if any link row already uses <paramref name="code"/> as its
    /// <see cref="Link.ShortCode"/> or <see cref="Link.CustomAlias"/>.</summary>
    Task<bool> CodeOrAliasExistsAsync(string code, CancellationToken cancellationToken = default);

    /// <summary>Persists a new <see cref="Link"/> and returns it with the database-assigned id.</summary>
    Task<Link> AddLinkAsync(Link link, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns the <see cref="Link"/> whose <see cref="Link.ShortCode"/> or
    /// <see cref="Link.CustomAlias"/> matches <paramref name="shortCode"/>, or
    /// <c>null</c> if no such link exists.
    /// </summary>
    Task<Link?> GetByShortCodeAsync(string shortCode, CancellationToken cancellationToken = default);

    /// <summary>Returns all <see cref="Link"/> rows, ordered by creation date (newest first).</summary>
    Task<List<Link>> GetAllLinksAsync(CancellationToken cancellationToken = default);

    /// <summary>Returns the <see cref="Link"/> with the given primary key, or <c>null</c>.</summary>
    Task<Link?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>Persists changes made to an existing <see cref="Link"/> and returns the updated entity.</summary>
    Task<Link> UpdateLinkAsync(Link link, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes the <see cref="Link"/> with the given primary key.
    /// Returns <c>true</c> if the row existed and was deleted; <c>false</c> if it was not found.
    /// </summary>
    Task<bool> DeleteLinkAsync(int id, CancellationToken cancellationToken = default);
}
