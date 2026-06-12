using BSDCPolls.Api.Data.Entities;

namespace BSDCPolls.Api.Data.Repositories;

/// <summary>Data-access contract for <see cref="UsernameHistory"/> persistence operations.</summary>
public interface IUsernameHistoryRepository
{
    /// <summary>Persists a new <see cref="UsernameHistory"/> entry.</summary>
    Task AddAsync(UsernameHistory entry, CancellationToken ct = default);

    /// <summary>
    /// Returns <c>true</c> if <paramref name="username"/> appears in the history of user
    /// <paramref name="userId"/> within the last <paramref name="days"/> days,
    /// preventing reuse of recently-held usernames.
    /// </summary>
    Task<bool> IsUsernameRecentlyUsedAsync(
        string username,
        int userId,
        int days,
        CancellationToken ct = default);

    /// <summary>
    /// Returns the count of username changes made by <paramref name="userId"/> within
    /// the last <paramref name="days"/> days. Used for rate-limit enforcement.
    /// </summary>
    Task<int> CountRecentChangesAsync(int userId, int days, CancellationToken ct = default);
}
