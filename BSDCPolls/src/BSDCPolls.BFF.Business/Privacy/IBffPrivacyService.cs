using BSDCPolls.Contracts.Requests.Privacy;
using BSDCPolls.Contracts.Responses.Users;

namespace BSDCPolls.BFF.Business.Privacy;

/// <summary>BFF service that forwards privacy and user profile requests to the internal API.</summary>
public interface IBffPrivacyService
{
    /// <summary>Returns the current user's privacy settings and allowlist.</summary>
    Task<PrivacySettingsResponse> GetPrivacySettingsAsync(
        string bearerToken,
        CancellationToken ct = default
    );

    /// <summary>Updates the current user's privacy settings.</summary>
    Task<PrivacySettingsResponse> UpdatePrivacySettingsAsync(
        UpdatePrivacySettingsRequest request,
        string bearerToken,
        CancellationToken ct = default
    );

    /// <summary>Adds a user to the current user's invite allowlist.</summary>
    Task<AllowlistEntryResponse> AddAllowlistEntryAsync(
        AddAllowlistEntryRequest request,
        string bearerToken,
        CancellationToken ct = default
    );

    /// <summary>Removes a user from the current user's invite allowlist.</summary>
    Task RemoveAllowlistEntryAsync(
        Guid allowedUserUid,
        string bearerToken,
        CancellationToken ct = default
    );
}
