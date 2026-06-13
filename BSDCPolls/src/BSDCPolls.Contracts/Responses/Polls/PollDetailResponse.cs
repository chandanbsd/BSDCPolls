using BSDCPolls.Contracts.Enums;

namespace BSDCPolls.Contracts.Responses.Polls;

/// <summary>Full poll details including all questions and options.</summary>
/// <param name="PollUid">Public GUID identifier.</param>
/// <param name="Title">Poll title.</param>
/// <param name="IsPublic">Whether the poll is visible to all users.</param>
/// <param name="Status">Current poll status.</param>
/// <param name="CreatedOn">UTC creation timestamp.</param>
/// <param name="IsCreator">True when the requesting user owns this poll.</param>
/// <param name="Questions">All questions (pushed and staged).</param>
public sealed record PollDetailResponse(
    Guid PollUid,
    string Title,
    bool IsPublic,
    PollStatus Status,
    DateTime CreatedOn,
    bool IsCreator,
    IReadOnlyList<PollQuestionResponse> Questions);
