using BSDCPolls.Contracts.Enums;
using BSDCPolls.Contracts.Requests.Polls;
using BSDCPolls.Contracts.Responses.Polls;

namespace BSDCPolls.Api.Business.Polls;

/// <summary>Domain service for poll lifecycle, question management, and voting.</summary>
public interface IPollService
{
    /// <summary>Creates a new poll in Draft status owned by <paramref name="creatorId"/>.</summary>
    Task<PollDetailResponse> CreateAsync(CreatePollRequest request, int creatorId, CancellationToken ct = default);

    /// <summary>Returns full poll details if the requesting user is authorized to view it.</summary>
    Task<PollDetailResponse> GetByUidAsync(Guid uid, int requestingUserId, CancellationToken ct = default);

    /// <summary>Returns a paginated feed of polls visible to <paramref name="userId"/>.</summary>
    Task<PollFeedResponse> GetFeedAsync(
        int userId,
        bool showPublic,
        PollStatus? status,
        int page,
        int pageSize,
        CancellationToken ct = default);

    /// <summary>Adds a question to the poll; optionally pushes it immediately.</summary>
    Task<PollQuestionResponse> AddQuestionAsync(
        Guid pollUid,
        AddPollQuestionRequest request,
        int creatorId,
        CancellationToken ct = default);

    /// <summary>Pushes a previously staged question to live participants.</summary>
    Task<PollQuestionResponse> PushQuestionAsync(Guid pollUid, Guid questionUid, int creatorId, CancellationToken ct = default);

    /// <summary>Changes the poll status (Active or Closed).</summary>
    Task<PollDetailResponse> ChangeStatusAsync(Guid pollUid, PollStatus newStatus, int creatorId, CancellationToken ct = default);

    /// <summary>
    /// Records a participant's vote. Validates the question is pushed, the poll is Active,
    /// and the user has not already voted on this question.
    /// </summary>
    Task<PollSubmissionResponse> SubmitVoteAsync(Guid pollUid, SubmitPollVoteRequest request, int respondentId, CancellationToken ct = default);

    /// <summary>Returns aggregated vote counts. Only the creator may access results during an active poll.</summary>
    Task<PollResultsResponse> GetResultsAsync(Guid pollUid, int requestingUserId, CancellationToken ct = default);
}
