using BSDCPolls.Contracts.Requests.Privacy;
using BSDCPolls.Contracts.Responses.Users;

namespace BSDCPolls.Api.Business.Privacy;

/// <summary>Domain service for managing user privacy settings and invite allowlists.</summary>
public interface IPrivacyService
{
    /// <summary>Returns the privacy settings for the given user, creating defaults if none exist.</summary>
    Task<PrivacySettingsResponse> GetSettingsAsync(int userId, CancellationToken ct = default);

    /// <summary>Updates the privacy settings for the given user.</summary>
    Task<PrivacySettingsResponse> UpdateSettingsAsync(
        int userId,
        UpdatePrivacySettingsRequest request,
        CancellationToken ct = default
    );

    /// <summary>
    /// Adds a user identified by <paramref name="targetUsername"/> to the owner's invite allowlist.
    /// Validates that the target user exists before adding.
    /// </summary>
    Task<AllowlistEntryResponse> AddAllowlistEntryAsync(
        int ownerId,
        string targetUsername,
        CancellationToken ct = default
    );

    /// <summary>Removes an allowlist entry identified by the allowed user's public UID.</summary>
    Task RemoveAllowlistEntryAsync(
        int ownerId,
        Guid allowedUserUid,
        CancellationToken ct = default
    );
}
