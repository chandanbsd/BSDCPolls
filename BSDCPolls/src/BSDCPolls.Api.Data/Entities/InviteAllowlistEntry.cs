namespace BSDCPolls.Api.Data.Entities;

/// <summary>
/// Records a specific user who is permitted to invite the owning user when
/// their <see cref="UserPrivacySettings.InvitePermission"/> is set to
/// <c>AllowlistOnly</c>.
/// </summary>
public class InviteAllowlistEntry : AuditableEntity
{
    /// <summary>FK to the user who maintains this allowlist.</summary>
    public int OwnerId { get; private set; }

    /// <summary>Navigation property to the allowlist owner.</summary>
    public virtual ApplicationUser Owner { get; private set; } = null!;

    /// <summary>FK to the user who is permitted to send invitations to the owner.</summary>
    public int AllowedUserId { get; private set; }

    /// <summary>Navigation property to the allowed user.</summary>
    public virtual ApplicationUser AllowedUser { get; private set; } = null!;

    /// <summary>EF Core proxy constructor.</summary>
    protected InviteAllowlistEntry()
    {
    }

    private InviteAllowlistEntry(int ownerId, int allowedUserId)
    {
        InitialiseIdentity(Guid.NewGuid());
        OwnerId = ownerId;
        AllowedUserId = allowedUserId;
    }

    /// <summary>Adds a user to the owner's invite allowlist.</summary>
    /// <param name="ownerId">The internal ID of the allowlist owner.</param>
    /// <param name="allowedUserId">The internal ID of the user being granted invite permission.</param>
    /// <returns>A new <see cref="InviteAllowlistEntry"/> ready for persistence.</returns>
    public static InviteAllowlistEntry Create(int ownerId, int allowedUserId)
    {
        return new InviteAllowlistEntry(ownerId, allowedUserId);
    }

    /// <summary>Soft-removes this allowlist entry, revoking invite permission.</summary>
    public void Deactivate()
    {
        MarkInactive();
    }
}
