using BSDCPolls.Contracts.Enums;

namespace BSDCPolls.Api.Data.Entities;

/// <summary>
/// Per-user privacy configuration. Created automatically when a user registers.
/// Controls feed visibility and who may invite the user to polls and surveys.
/// </summary>
public class UserPrivacySettings : AuditableEntity
{
    /// <summary>FK to the user who owns these settings. One-to-one with ApplicationUser.</summary>
    public int UserId { get; private set; }

    /// <summary>Navigation property to the owning user.</summary>
    public virtual ApplicationUser User { get; private set; } = null!;

    /// <summary>
    /// Whether this user's public polls and surveys appear in other users' feeds.
    /// Defaults to true on registration.
    /// </summary>
    public bool ShowPublicContent { get; private set; }

    /// <summary>Controls who may invite this user to polls and surveys.</summary>
    public InvitePermission InvitePermission { get; private set; }

    /// <summary>EF Core proxy constructor.</summary>
    protected UserPrivacySettings() { }

    private UserPrivacySettings(int userId)
    {
        InitialiseIdentity(Guid.NewGuid());
        UserId = userId;
        ShowPublicContent = true;
        InvitePermission = InvitePermission.Everyone;
    }

    /// <summary>Creates default privacy settings for a newly registered user.</summary>
    /// <param name="userId">The internal ID of the user.</param>
    /// <returns>A new <see cref="UserPrivacySettings"/> with permissive defaults.</returns>
    public static UserPrivacySettings CreateDefault(int userId)
    {
        return new UserPrivacySettings(userId);
    }

    /// <summary>Applies updated privacy preferences.</summary>
    /// <param name="showPublicContent">Whether public content appears in the user's feed.</param>
    /// <param name="invitePermission">Who may invite this user.</param>
    public void Update(bool showPublicContent, InvitePermission invitePermission)
    {
        ShowPublicContent = showPublicContent;
        InvitePermission = invitePermission;
    }
}
