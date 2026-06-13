using BSDCPolls.Contracts.Enums;
using BSDCPolls.Contracts.Requests.Polls;
using BSDCPolls.Contracts.Responses.Polls;

namespace BSDCPolls.BFF.Business.Polls;

/// <summary>BFF service that forwards poll REST requests to the internal API.</summary>
public interface IBffPollService
{
    /// <summary>Creates a new poll and returns its details.</summary>
    Task<PollDetailResponse> CreateAsync(CreatePollRequest request, string bearerToken, CancellationToken ct = default);

    /// <summary>Returns full poll details for the given poll UID.</summary>
    Task<PollDetailResponse> GetByUidAsync(Guid pollUid, string bearerToken, CancellationToken ct = default);

    /// <summary>Returns a paginated feed of polls visible to the current user.</summary>
    Task<PollFeedResponse> GetFeedAsync(PollStatus? status, int page, int pageSize, string bearerToken, CancellationToken ct = default);

    /// <summary>Changes the poll status.</summary>
    Task<PollDetailResponse> ChangeStatusAsync(Guid pollUid, ChangePollStatusRequest request, string bearerToken, CancellationToken ct = default);

    /// <summary>Adds a question to the poll and optionally pushes it immediately.</summary>
    Task<PollQuestionResponse> AddQuestionAsync(Guid pollUid, AddPollQuestionRequest request, string bearerToken, CancellationToken ct = default);

    /// <summary>Pushes a previously staged question to live participants.</summary>
    Task<PollQuestionResponse> PushQuestionAsync(Guid pollUid, Guid questionUid, string bearerToken, CancellationToken ct = default);

    /// <summary>Returns aggregated vote counts for the poll.</summary>
    Task<PollResultsResponse> GetResultsAsync(Guid pollUid, string bearerToken, CancellationToken ct = default);
}
