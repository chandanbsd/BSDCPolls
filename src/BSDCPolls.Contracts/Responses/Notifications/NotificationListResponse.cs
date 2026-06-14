namespace BSDCPolls.Contracts.Responses.Notifications;

/// <summary>Paginated list of notifications for the current user.</summary>
/// <param name="UnreadCount">Total count of unread notifications across all pages.</param>
/// <param name="Items">Current page of notification items.</param>
/// <param name="TotalCount">Total notification count across all pages.</param>
/// <param name="Page">Current page number (1-based).</param>
/// <param name="PageSize">Number of items per page.</param>
public sealed record NotificationListResponse(
    int UnreadCount,
    IReadOnlyList<NotificationItem> Items,
    int TotalCount,
    int Page,
    int PageSize
);
