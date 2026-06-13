using BSDCPolls.Contracts.Responses.Notifications;

namespace BSDCPolls.BFF.Controllers.Internal;

/// <summary>Request body for the notification push endpoint.</summary>
/// <param name="TargetSupabaseId">The SupabaseUserId (email claim) of the notification recipient.</param>
/// <param name="Payload">The invitation event payload to deliver.</param>
public sealed record NotificationPushRequest(
    string TargetSupabaseId,
    InvitationReceivedPayload Payload
);
