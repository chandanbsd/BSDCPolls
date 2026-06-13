namespace BSDCPolls.Contracts.Responses.Polls;

/// <summary>A single question within a poll response.</summary>
/// <param name="QuestionUid">Public GUID identifier for the question.</param>
/// <param name="Text">Question text.</param>
/// <param name="OrderIndex">Sequence index.</param>
/// <param name="PushedAt">UTC time when the question was pushed to participants; null if staged.</param>
/// <param name="Options">Answer options.</param>
public sealed record PollQuestionResponse(
    Guid QuestionUid,
    string Text,
    int OrderIndex,
    DateTime? PushedAt,
    IReadOnlyList<PollAnswerOptionResponse> Options);
