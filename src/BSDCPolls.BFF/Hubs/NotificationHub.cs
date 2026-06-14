using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace BSDCPolls.BFF.Hubs;

/// <summary>
/// SignalR hub for per-user in-app notifications. Each authenticated user is added to a
/// personal group keyed by their SupabaseUserId (the JWT email claim). The server pushes
/// <c>InvitationReceived</c> events to the group when an invitation is created.
/// </summary>
[Authorize]
public sealed class NotificationHub : Hub<INotificationHub>
{
    private readonly ILogger<NotificationHub> _logger;

    /// <summary>Initialises the hub with a logger.</summary>
    public NotificationHub(ILogger<NotificationHub> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc />
    public override async Task OnConnectedAsync()
    {
        var supabaseUserId = Context.User?.FindFirstValue("email");
        if (string.IsNullOrEmpty(supabaseUserId))
        {
            throw new HubException("Unauthenticated.");
        }

        await Groups.AddToGroupAsync(Context.ConnectionId, supabaseUserId);

        _logger.LogInformation(
            "Connection {ConnectionId} joined notification group for user {SupabaseUserId}",
            Context.ConnectionId,
            supabaseUserId
        );

        await base.OnConnectedAsync();
    }

    /// <inheritdoc />
    public override Task OnDisconnectedAsync(Exception? exception)
    {
        return base.OnDisconnectedAsync(exception);
    }
}
