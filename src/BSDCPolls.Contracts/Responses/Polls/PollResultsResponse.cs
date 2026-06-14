using BSDCPolls.Contracts.Enums;

namespace BSDCPolls.Contracts.Responses.Polls;

/// <summary>Aggregated vote results for an entire poll.</summary>
/// <param name="PollUid">Public GUID of the poll.</param>
/// <param name="Title">Poll title.</param>
/// <param name="Status">Current poll status.</param>
/// <param name="Questions">Per-question vote breakdowns.</param>
public sealed record PollResultsResponse(
    Guid PollUid,
    string Title,
    PollStatus Status,
    IReadOnlyList<PollResultsQuestionResponse> Questions
);
