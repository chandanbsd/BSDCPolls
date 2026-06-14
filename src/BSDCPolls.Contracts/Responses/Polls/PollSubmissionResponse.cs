namespace BSDCPolls.Contracts.Responses.Polls;

/// <summary>Confirmation of a successfully recorded poll vote.</summary>
/// <param name="SubmissionUid">Public GUID of the new submission record.</param>
/// <param name="QuestionUid">The question that was answered.</param>
/// <param name="SelectedOptionUid">The chosen answer option.</param>
/// <param name="SubmittedAt">UTC timestamp of submission.</param>
public sealed record PollSubmissionResponse(
    Guid SubmissionUid,
    Guid QuestionUid,
    Guid SelectedOptionUid,
    DateTime SubmittedAt
);
