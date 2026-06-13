using BSDCPolls.BFF.Business.Notifications;
using BSDCPolls.Contracts.Responses.Notifications;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BSDCPolls.BFF.Controllers;

/// <summary>Public-facing notification endpoints for the Angular frontend.</summary>
[ApiController]
[Route("api/notifications")]
[Authorize]
public sealed class NotificationsController : ControllerBase
{
    private readonly IBffNotificationService _notificationService;

    /// <summary>Initialises the controller with the BFF notification service.</summary>
    public NotificationsController(IBffNotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    /// <summary>Returns a paginated list of notifications for the current user.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(NotificationListResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetNotifications(
        [FromQuery] bool unreadOnly = false,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default
    )
    {
        var token = ExtractBearerToken();
        if (token is null)
        {
            return Unauthorized();
        }

        var result = await _notificationService.GetNotificationsAsync(
            unreadOnly,
            page,
            pageSize,
            token,
            ct
        );
        return Ok(result);
    }

    /// <summary>Marks a specific notification as read.</summary>
    [HttpPatch("{notificationUid:guid}/read")]
    [ProducesResponseType(typeof(NotificationReadResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> MarkRead(Guid notificationUid, CancellationToken ct)
    {
        var token = ExtractBearerToken();
        if (token is null)
        {
            return Unauthorized();
        }

        var result = await _notificationService.MarkReadAsync(notificationUid, token, ct);
        return Ok(result);
    }

    /// <summary>Marks all notifications as read for the current user.</summary>
    [HttpPatch("read-all")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> MarkAllRead(CancellationToken ct)
    {
        var token = ExtractBearerToken();
        if (token is null)
        {
            return Unauthorized();
        }

        await _notificationService.MarkAllReadAsync(token, ct);
        return NoContent();
    }

    private string? ExtractBearerToken()
    {
        var authHeader = HttpContext.Request.Headers.Authorization.ToString();
        if (
            string.IsNullOrEmpty(authHeader)
            || !authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)
        )
        {
            return null;
        }

        return authHeader["Bearer ".Length..];
    }
}
