using BSDCPolls.Contracts.Enums;

namespace BSDCPolls.Contracts.Requests.Privacy;

/// <summary>Request body for updating the current user's privacy settings.</summary>
/// <param name="ShowPublicContent">Whether the user's public content appears in other users' feeds.</param>
/// <param name="InvitePermission">Who is permitted to invite this user to polls and surveys.</param>
public sealed record UpdatePrivacySettingsRequest(
    bool ShowPublicContent,
    InvitePermission InvitePermission
);
