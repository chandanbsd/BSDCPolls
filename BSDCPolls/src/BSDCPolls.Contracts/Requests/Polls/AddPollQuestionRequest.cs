using System.ComponentModel.DataAnnotations;

namespace BSDCPolls.Contracts.Requests.Polls;

/// <summary>Payload for adding a question to a poll.</summary>
/// <param name="Text">Question text; 1–500 characters.</param>
/// <param name="Options">Answer options; 2–10 items.</param>
/// <param name="PushImmediately">When true the question is pushed live immediately after creation.</param>
public sealed record AddPollQuestionRequest(
    [Required] [MaxLength(500)] string Text,
    [Required] IReadOnlyList<AnswerOptionInput> Options,
    bool PushImmediately
);
