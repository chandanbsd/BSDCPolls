namespace BSDCPolls.Contracts.Responses.Notifications;

/// <summary>A single notification entry for the current user.</summary>
/// <param name="NotificationUid">Public GUID of the notification.</param>
/// <param name="IsRead">Whether the notification has been acknowledged.</param>
/// <param name="CreatedOn">UTC timestamp the notification was created.</param>
/// <param name="Invitation">Details about the invitation that triggered this notification.</param>
public sealed record NotificationItem(
    Guid NotificationUid,
    bool IsRead,
    DateTime CreatedOn,
    NotificationInvitationDetail Invitation
);
