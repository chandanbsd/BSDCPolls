using BSDCPolls.Api.Data.Entities;

namespace BSDCPolls.Api.Data.Repositories;

/// <summary>Data-access contract for <see cref="UserPrivacySettings"/> and <see cref="InviteAllowlistEntry"/> persistence.</summary>
public interface IPrivacySettingsRepository
{
    /// <summary>Returns the privacy settings for the given user, or <c>null</c> if not yet created.</summary>
    Task<UserPrivacySettings?> GetByUserIdAsync(int userId, CancellationToken ct = default);

    /// <summary>Persists a new <paramref name="settings"/> record and returns the tracked instance.</summary>
    Task<UserPrivacySettings> CreateAsync(UserPrivacySettings settings, CancellationToken ct = default);

    /// <summary>Saves any pending changes to <paramref name="settings"/> tracked by the current DbContext.</summary>
    Task UpdateAsync(UserPrivacySettings settings, CancellationToken ct = default);

    /// <summary>Returns all active allowlist entries for the given owner, including the allowed user navigation.</summary>
    Task<IReadOnlyList<InviteAllowlistEntry>> GetAllowlistAsync(int ownerId, CancellationToken ct = default);

    /// <summary>Returns the allowlist entry matching the owner/allowed-user pair, or <c>null</c>.</summary>
    Task<InviteAllowlistEntry?> GetAllowlistEntryAsync(int ownerId, int allowedUserId, CancellationToken ct = default);

    /// <summary>Persists a new <paramref name="entry"/> and returns the tracked instance.</summary>
    Task<InviteAllowlistEntry> AddAllowlistEntryAsync(InviteAllowlistEntry entry, CancellationToken ct = default);

    /// <summary>Returns an active allowlist entry by the owner and the allowed user's public UID, or <c>null</c>.</summary>
    Task<InviteAllowlistEntry?> GetAllowlistEntryByUidAsync(int ownerId, Guid allowedUserUid, CancellationToken ct = default);

    /// <summary>Saves pending changes to <paramref name="entry"/> (e.g. after deactivation).</summary>
    Task UpdateAllowlistEntryAsync(InviteAllowlistEntry entry, CancellationToken ct = default);
}
