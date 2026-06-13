using BSDCPolls.BFF.Hubs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace BSDCPolls.BFF.Controllers.Internal;

/// <summary>
/// Internal-only endpoint for the backend API to trigger SignalR notification pushes.
/// Not JWT-authenticated — access is restricted to the internal Aspire service mesh.
/// </summary>
[ApiController]
[Route("internal/notifications")]
[AllowAnonymous]
public sealed class NotificationPushController : ControllerBase
{
    private readonly IHubContext<NotificationHub> _hubContext;
    private readonly ILogger<NotificationPushController> _logger;

    /// <summary>Initialises the controller with the notification hub context and logger.</summary>
    public NotificationPushController(IHubContext<NotificationHub> hubContext, ILogger<NotificationPushController> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }

    /// <summary>Pushes an <c>InvitationReceived</c> event to the target user's SignalR group.</summary>
    [HttpPost("push")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Push([FromBody] NotificationPushRequest request, CancellationToken ct)
    {
        await _hubContext.Clients
            .Group(request.TargetSupabaseId)
            .SendAsync("InvitationReceived", request.Payload, ct);

        _logger.LogInformation(
            "Pushed InvitationReceived to group {TargetSupabaseId}",
            request.TargetSupabaseId);

        return NoContent();
    }
}
