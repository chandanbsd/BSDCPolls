namespace BSDCPolls.Api.Data.Entities;

/// <summary>
/// An in-app alert generated when a user receives an invitation.
/// One notification is created per invitation at invitation creation time.
/// </summary>
public class Notification : AuditableEntity
{
    /// <summary>FK to the user who should see this notification.</summary>
    public int RecipientId { get; private set; }

    /// <summary>Navigation property to the recipient.</summary>
    public virtual ApplicationUser Recipient { get; private set; } = null!;

    /// <summary>FK to the invitation that triggered this notification.</summary>
    public int InvitationId { get; private set; }

    /// <summary>Navigation property to the linked invitation.</summary>
    public virtual Invitation Invitation { get; private set; } = null!;

    /// <summary>Whether the recipient has acknowledged this notification.</summary>
    public bool IsRead { get; private set; }

    /// <summary>UTC time the notification was marked as read. <c>null</c> until read.</summary>
    public DateTime? ReadAt { get; private set; }

    /// <summary>EF Core proxy constructor.</summary>
    protected Notification() { }

    private Notification(int recipientId, int invitationId)
    {
        InitialiseIdentity(Guid.NewGuid());
        RecipientId = recipientId;
        InvitationId = invitationId;
        IsRead = false;
    }

    /// <summary>Creates a notification for the invitee when an invitation is created.</summary>
    /// <param name="recipientId">Internal ID of the notification recipient.</param>
    /// <param name="invitationId">Internal ID of the linked invitation.</param>
    /// <returns>A new <see cref="Notification"/> in unread state.</returns>
    public static Notification Create(int recipientId, int invitationId)
    {
        return new Notification(recipientId, invitationId);
    }

    /// <summary>Marks this notification as read.</summary>
    /// <param name="readAt">UTC time the notification was acknowledged.</param>
    public void MarkRead(DateTime readAt)
    {
        if (IsRead)
        {
            return;
        }

        IsRead = true;
        ReadAt = readAt;
    }
}
