namespace BSDCPolls.Contracts.Enums;

/// <summary>Lifecycle status of an invitation.</summary>
public enum InvitationStatus
{
    /// <summary>Invitation has been sent but not yet viewed.</summary>
    Pending = 0,

    /// <summary>Invitation has been viewed by the invitee.</summary>
    Viewed = 1,

    /// <summary>Invitation has been declined by the invitee.</summary>
    Declined = 2,
}
