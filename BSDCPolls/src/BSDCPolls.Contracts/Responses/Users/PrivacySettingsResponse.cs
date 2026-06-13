using BSDCPolls.Contracts.Enums;

namespace BSDCPolls.Contracts.Responses.Users;

/// <summary>The current user's privacy settings including their invite allowlist.</summary>
/// <param name="ShowPublicContent">Whether the user's public content appears in other users' feeds.</param>
/// <param name="InvitePermission">Who is permitted to invite this user.</param>
/// <param name="Allowlist">Users explicitly permitted to send invitations when InvitePermission is AllowlistOnly.</param>
public sealed record PrivacySettingsResponse(
    bool ShowPublicContent,
    InvitePermission InvitePermission,
    IReadOnlyList<AllowlistEntryResponse> Allowlist);
