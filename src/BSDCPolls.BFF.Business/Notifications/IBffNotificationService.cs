using BSDCPolls.Contracts.Responses.Notifications;

namespace BSDCPolls.BFF.Business.Notifications;

/// <summary>BFF service that forwards notification requests to the internal API.</summary>
public interface IBffNotificationService
{
    /// <summary>Returns a paginated list of notifications for the current user.</summary>
    Task<NotificationListResponse> GetNotificationsAsync(
        bool unreadOnly,
        int page,
        int pageSize,
        string bearerToken,
        CancellationToken ct = default
    );

    /// <summary>Marks a specific notification as read.</summary>
    Task<NotificationReadResponse> MarkReadAsync(
        Guid notificationUid,
        string bearerToken,
        CancellationToken ct = default
    );

    /// <summary>Marks all notifications as read for the current user.</summary>
    Task MarkAllReadAsync(string bearerToken, CancellationToken ct = default);

    /// <summary>Returns the unread notification count for the current user.</summary>
    Task<int> GetUnreadCountAsync(string bearerToken, CancellationToken ct = default);
}
