using BSDCPolls.Contracts.Responses.Polls;

namespace BSDCPolls.Contracts.SignalR;

/// <summary>Broadcast when the creator pushes a question to live participants.</summary>
/// <param name="PollUid">The poll this event belongs to.</param>
/// <param name="Question">Full question details including options.</param>
public sealed record PollQuestionPushedPayload(Guid PollUid, PollQuestionResponse Question);
