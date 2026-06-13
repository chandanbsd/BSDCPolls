using BSDCPolls.Api.Data.Entities;

namespace BSDCPolls.Api.Data.Repositories;

/// <summary>Data-access contract for <see cref="ApplicationUser"/> persistence operations.</summary>
public interface IUserRepository
{
    /// <summary>Returns the user whose <see cref="ApplicationUser.SupabaseUserId"/> matches <paramref name="supabaseUserId"/>, or <c>null</c>.</summary>
    Task<ApplicationUser?> GetBySupabaseIdAsync(
        string supabaseUserId,
        CancellationToken ct = default
    );

    /// <summary>Returns the user with the given <paramref name="username"/>, or <c>null</c>.</summary>
    Task<ApplicationUser?> GetByUsernameAsync(string username, CancellationToken ct = default);

    /// <summary>Returns the user with the given public <paramref name="uid"/>, or <c>null</c>.</summary>
    Task<ApplicationUser?> GetByUidAsync(Guid uid, CancellationToken ct = default);

    /// <summary>Returns <c>true</c> if <paramref name="username"/> is already taken by any active user.</summary>
    Task<bool> UsernameExistsAsync(string username, CancellationToken ct = default);

    /// <summary>Persists a newly created <paramref name="user"/> and returns the tracked instance.</summary>
    Task<ApplicationUser> CreateAsync(ApplicationUser user, CancellationToken ct = default);

    /// <summary>Saves any pending changes to <paramref name="user"/> tracked by the current DbContext.</summary>
    Task UpdateAsync(ApplicationUser user, CancellationToken ct = default);
}
