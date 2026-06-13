using BSDCPolls.Contracts.Responses.Notifications;

namespace BSDCPolls.Api.Business.Notifications;

/// <summary>Domain service for reading and managing in-app notifications.</summary>
public interface INotificationService
{
    /// <summary>
    /// Returns a paginated notification list for <paramref name="recipientId"/>,
    /// optionally filtered to unread only.
    /// </summary>
    Task<NotificationListResponse> GetNotificationsAsync(
        int recipientId,
        bool unreadOnly,
        int page,
        int pageSize,
        CancellationToken ct = default
    );

    /// <summary>Marks a single notification as read. Returns <c>null</c> if not found or not owned by the recipient.</summary>
    Task<NotificationReadResponse?> MarkReadAsync(
        Guid notificationUid,
        int recipientId,
        CancellationToken ct = default
    );

    /// <summary>Marks all notifications as read for <paramref name="recipientId"/>. Returns the count of notifications updated.</summary>
    Task<int> MarkAllReadAsync(int recipientId, CancellationToken ct = default);

    /// <summary>Returns the number of unread notifications for <paramref name="recipientId"/>.</summary>
    Task<int> GetUnreadCountAsync(int recipientId, CancellationToken ct = default);
}
