using BSDCPolls.Contracts.Enums;

namespace BSDCPolls.Contracts.SignalR;

/// <summary>Broadcast to all group members when the poll status changes.</summary>
/// <param name="PollUid">The poll whose status changed.</param>
/// <param name="NewStatus">The new poll status.</param>
public sealed record PollStatusChangedPayload(Guid PollUid, PollStatus NewStatus);
