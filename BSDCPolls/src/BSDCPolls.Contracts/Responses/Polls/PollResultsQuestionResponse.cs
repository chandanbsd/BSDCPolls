namespace BSDCPolls.Contracts.Responses.Polls;

/// <summary>Vote count breakdown for a single poll question.</summary>
/// <param name="QuestionUid">Public GUID of the question.</param>
/// <param name="Text">Question text.</param>
/// <param name="PushedAt">UTC time the question was pushed; null if never pushed.</param>
/// <param name="TotalVotes">Total votes cast for this question.</param>
/// <param name="Options">Per-option vote breakdown.</param>
public sealed record PollResultsQuestionResponse(
    Guid QuestionUid,
    string Text,
    DateTime? PushedAt,
    int TotalVotes,
    IReadOnlyList<PollResultsOptionResponse> Options
);
