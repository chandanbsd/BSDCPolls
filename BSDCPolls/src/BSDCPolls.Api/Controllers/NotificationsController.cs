using System.Security.Claims;
using BSDCPolls.Api.Business.Notifications;
using BSDCPolls.Api.Data.Repositories;
using BSDCPolls.Contracts.Responses.Notifications;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BSDCPolls.Api.Controllers;

/// <summary>Internal API endpoints for reading and managing in-app notifications.</summary>
[ApiController]
[Route("api/internal/notifications")]
[Authorize]
public sealed class NotificationsController : ControllerBase
{
    private readonly INotificationService _notificationService;
    private readonly IUserRepository _userRepository;

    /// <summary>Initialises the controller with the notification service and user repository.</summary>
    public NotificationsController(INotificationService notificationService, IUserRepository userRepository)
    {
        _notificationService = notificationService;
        _userRepository = userRepository;
    }

    /// <summary>Returns a paginated list of notifications for the current user.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(NotificationListResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetNotifications(
        [FromQuery] bool unreadOnly = false,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var user = await GetCurrentUserAsync(ct);
        if (user is null)
        {
            return Unauthorized();
        }

        var result = await _notificationService.GetNotificationsAsync(user.Id, unreadOnly, page, pageSize, ct);
        return Ok(result);
    }

    /// <summary>Marks a specific notification as read.</summary>
    [HttpPatch("{notificationUid:guid}/read")]
    [ProducesResponseType(typeof(NotificationReadResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> MarkRead(Guid notificationUid, CancellationToken ct)
    {
        var user = await GetCurrentUserAsync(ct);
        if (user is null)
        {
            return Unauthorized();
        }

        var result = await _notificationService.MarkReadAsync(notificationUid, user.Id, ct);
        if (result is null)
        {
            return NotFound();
        }

        return Ok(result);
    }

    /// <summary>Marks all notifications as read for the current user.</summary>
    [HttpPatch("read-all")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> MarkAllRead(CancellationToken ct)
    {
        var user = await GetCurrentUserAsync(ct);
        if (user is null)
        {
            return Unauthorized();
        }

        await _notificationService.MarkAllReadAsync(user.Id, ct);
        return NoContent();
    }

    private async Task<BSDCPolls.Api.Data.Entities.ApplicationUser?> GetCurrentUserAsync(CancellationToken ct)
    {
        var supabaseUserId = User.FindFirstValue("email");
        if (string.IsNullOrEmpty(supabaseUserId))
        {
            return null;
        }

        return await _userRepository.GetBySupabaseIdAsync(supabaseUserId, ct);
    }
}
