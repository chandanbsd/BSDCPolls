using BSDCPolls.Api.Data.Entities;
using BSDCPolls.Api.Data.Repositories;
using BSDCPolls.Contracts.Responses.Notifications;
using Microsoft.Extensions.Logging;

namespace BSDCPolls.Api.Business.Notifications;

/// <summary>Domain service for reading and managing in-app notifications.</summary>
public sealed class NotificationService : INotificationService
{
    private readonly INotificationRepository _notificationRepository;
    private readonly ILogger<NotificationService> _logger;

    /// <summary>Initialises the service with required repositories and logger.</summary>
    public NotificationService(INotificationRepository notificationRepository, ILogger<NotificationService> logger)
    {
        _notificationRepository = notificationRepository;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<NotificationListResponse> GetNotificationsAsync(
        int recipientId,
        bool unreadOnly,
        int page,
        int pageSize,
        CancellationToken ct = default)
    {
        var (items, totalCount, unreadCount) = await _notificationRepository
            .GetByRecipientAsync(recipientId, unreadOnly, page, pageSize, ct);

        return new NotificationListResponse(
            unreadCount,
            items.Select(MapToItem).ToList(),
            totalCount,
            page,
            pageSize);
    }

    /// <inheritdoc />
    public async Task<NotificationReadResponse?> MarkReadAsync(Guid notificationUid, int recipientId, CancellationToken ct = default)
    {
        var notification = await _notificationRepository.MarkReadAsync(notificationUid, recipientId, ct);

        if (notification is null)
        {
            return null;
        }

        _logger.LogInformation("Notification {NotificationUid} marked read by user {UserId}", notificationUid, recipientId);
        return new NotificationReadResponse(notification.Uid, notification.IsRead, notification.ReadAt!.Value);
    }

    /// <inheritdoc />
    public async Task<int> MarkAllReadAsync(int recipientId, CancellationToken ct = default)
    {
        var count = await _notificationRepository.MarkAllReadAsync(recipientId, ct);
        _logger.LogInformation("All notifications marked read for user {UserId} ({Count} updated)", recipientId, count);
        return count;
    }

    /// <inheritdoc />
    public Task<int> GetUnreadCountAsync(int recipientId, CancellationToken ct = default) =>
        _notificationRepository.GetUnreadCountAsync(recipientId, ct);

    private static NotificationItem MapToItem(Notification notification)
    {
        var invitation = notification.Invitation;
        var inviter = invitation.Inviter;

        return new NotificationItem(
            notification.Uid,
            notification.IsRead,
            notification.CreatedOn,
            new NotificationInvitationDetail(
                invitation.Uid,
                inviter.Username,
                invitation.Poll?.Uid,
                invitation.Poll?.Title,
                invitation.Survey?.Uid,
                invitation.Survey?.Title));
    }
}
