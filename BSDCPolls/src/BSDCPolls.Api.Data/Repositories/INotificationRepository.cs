using BSDCPolls.Api.Data.Entities;

namespace BSDCPolls.Api.Data.Repositories;

/// <summary>Data-access contract for <see cref="Notification"/> persistence operations.</summary>
public interface INotificationRepository
{
    /// <summary>Persists a new <paramref name="notification"/> and returns the tracked instance.</summary>
    Task<Notification> CreateAsync(Notification notification, CancellationToken ct = default);

    /// <summary>
    /// Returns a paginated list of notifications for <paramref name="recipientId"/>,
    /// optionally filtered to unread only.
    /// </summary>
    Task<(IReadOnlyList<Notification> Items, int TotalCount, int UnreadCount)> GetByRecipientAsync(
        int recipientId,
        bool unreadOnly,
        int page,
        int pageSize,
        CancellationToken ct = default);

    /// <summary>Returns the total number of unread notifications for <paramref name="recipientId"/>.</summary>
    Task<int> GetUnreadCountAsync(int recipientId, CancellationToken ct = default);

    /// <summary>Marks the notification with the given <paramref name="notificationUid"/> as read, if owned by <paramref name="recipientId"/>.</summary>
    Task<Notification?> MarkReadAsync(Guid notificationUid, int recipientId, CancellationToken ct = default);

    /// <summary>Marks all unread notifications for <paramref name="recipientId"/> as read. Returns the count of notifications marked.</summary>
    Task<int> MarkAllReadAsync(int recipientId, CancellationToken ct = default);
}
