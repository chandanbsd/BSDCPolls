namespace BSDCPolls.Contracts.Enums;

/// <summary>Invitation permission mode for a user's privacy settings.</summary>
public enum InvitePermission
{
    /// <summary>Accept invitations from all authenticated users.</summary>
    Everyone = 0,

    /// <summary>Reject all invitations.</summary>
    Nobody = 1,

    /// <summary>Accept invitations only from users on the allowlist.</summary>
    AllowlistOnly = 2,
}
