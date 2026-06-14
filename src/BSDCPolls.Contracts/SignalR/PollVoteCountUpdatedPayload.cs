using BSDCPolls.Contracts.Responses.Polls;

namespace BSDCPolls.Contracts.SignalR;

/// <summary>Broadcast to the creator only when a new vote is cast, updating running totals.</summary>
/// <param name="PollUid">The poll this event belongs to.</param>
/// <param name="QuestionUid">The question that received a new vote.</param>
/// <param name="Options">Updated per-option vote counts and percentages.</param>
public sealed record PollVoteCountUpdatedPayload(
    Guid PollUid,
    Guid QuestionUid,
    IReadOnlyList<PollResultsOptionResponse> Options
);
