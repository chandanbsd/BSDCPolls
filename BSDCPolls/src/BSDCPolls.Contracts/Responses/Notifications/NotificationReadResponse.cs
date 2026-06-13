namespace BSDCPolls.Contracts.Responses.Notifications;

/// <summary>Confirmation that a notification was marked as read.</summary>
/// <param name="NotificationUid">Public GUID of the updated notification.</param>
/// <param name="IsRead">Always true after a successful mark-read operation.</param>
/// <param name="ReadAt">UTC timestamp the notification was acknowledged.</param>
public sealed record NotificationReadResponse(
    Guid NotificationUid,
    bool IsRead,
    DateTime ReadAt);
