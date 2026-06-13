using BSDCPolls.Api.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace BSDCPolls.Api.Data.Repositories;

/// <summary>EF Core implementation of <see cref="INotificationRepository"/>.</summary>
public sealed class NotificationRepository : INotificationRepository
{
    private readonly BsdcPollsDbContext _db;

    /// <summary>Initialises the repository with the scoped <see cref="BsdcPollsDbContext"/>.</summary>
    public NotificationRepository(BsdcPollsDbContext db)
    {
        _db = db;
    }

    /// <inheritdoc />
    public async Task<Notification> CreateAsync(Notification notification, CancellationToken ct = default)
    {
        _db.Notifications.Add(notification);
        await _db.SaveChangesAsync(ct);
        return notification;
    }

    /// <inheritdoc />
    public async Task<(IReadOnlyList<Notification> Items, int TotalCount, int UnreadCount)> GetByRecipientAsync(
        int recipientId,
        bool unreadOnly,
        int page,
        int pageSize,
        CancellationToken ct = default)
    {
        var query = _db.Notifications
            .Include(n => n.Invitation)
                .ThenInclude(i => i.Inviter)
            .Include(n => n.Invitation)
                .ThenInclude(i => i.Poll)
            .Include(n => n.Invitation)
                .ThenInclude(i => i.Survey)
            .Where(n => n.RecipientId == recipientId && n.IsActive);

        if (unreadOnly)
        {
            query = query.Where(n => !n.IsRead);
        }

        var totalCount = await query.CountAsync(ct);
        var unreadCount = await _db.Notifications
            .CountAsync(n => n.RecipientId == recipientId && n.IsActive && !n.IsRead, ct);

        var items = await query
            .OrderByDescending(n => n.CreatedOn)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (items, totalCount, unreadCount);
    }

    /// <inheritdoc />
    public Task<int> GetUnreadCountAsync(int recipientId, CancellationToken ct = default) =>
        _db.Notifications.CountAsync(n => n.RecipientId == recipientId && n.IsActive && !n.IsRead, ct);

    /// <inheritdoc />
    public async Task<Notification?> MarkReadAsync(Guid notificationUid, int recipientId, CancellationToken ct = default)
    {
        var notification = await _db.Notifications
            .FirstOrDefaultAsync(n => n.Uid == notificationUid && n.RecipientId == recipientId && n.IsActive, ct);

        if (notification is null)
        {
            return null;
        }

        notification.MarkRead(DateTime.UtcNow);
        await _db.SaveChangesAsync(ct);
        return notification;
    }

    /// <inheritdoc />
    public async Task<int> MarkAllReadAsync(int recipientId, CancellationToken ct = default)
    {
        var unread = await _db.Notifications
            .Where(n => n.RecipientId == recipientId && n.IsActive && !n.IsRead)
            .ToListAsync(ct);

        if (unread.Count == 0)
        {
            return 0;
        }

        var now = DateTime.UtcNow;
        foreach (var notification in unread)
        {
            notification.MarkRead(now);
        }

        await _db.SaveChangesAsync(ct);
        return unread.Count;
    }
}
