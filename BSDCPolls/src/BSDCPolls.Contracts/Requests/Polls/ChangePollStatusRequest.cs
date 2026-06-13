using BSDCPolls.Contracts.Enums;

namespace BSDCPolls.Contracts.Requests.Polls;

/// <summary>Payload for changing the status of a poll.</summary>
/// <param name="Status">The new status; must be <c>Active</c> or <c>Closed</c>.</param>
public sealed record ChangePollStatusRequest(PollStatus Status);
