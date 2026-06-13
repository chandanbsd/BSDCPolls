using BSDCPolls.Api.Data.Entities;
using BSDCPolls.Contracts.Enums;

namespace BSDCPolls.Api.Data.Repositories;

/// <summary>Data-access contract for <see cref="Survey"/> persistence operations.</summary>
public interface ISurveyRepository
{
    /// <summary>
    /// Returns the survey with the given <paramref name="uid"/> or <c>null</c> if not found.
    /// </summary>
    /// <param name="uid">Public GUID of the survey.</param>
    /// <param name="requestingUserId">Internal ID of the requesting user (for access checks).</param>
    /// <param name="ct">Cancellation token.</param>
    Task<Survey?> GetByUidAsync(Guid uid, int? requestingUserId, CancellationToken ct = default);

    /// <summary>
    /// Returns a paginated list of surveys visible to <paramref name="userId"/>.
    /// Includes public surveys (when <paramref name="showPublic"/> is true) and
    /// surveys the user has been invited to.
    /// </summary>
    Task<(IReadOnlyList<Survey> Items, int TotalCount)> GetFeedAsync(
        int userId,
        bool showPublic,
        SurveyStatus? status,
        int page,
        int pageSize,
        CancellationToken ct = default
    );

    /// <summary>
    /// Returns the survey with all responses for results aggregation, restricted to the creator.
    /// </summary>
    Task<Survey?> GetResultsAsync(Guid surveyUid, int creatorId, CancellationToken ct = default);

    /// <summary>Persists a newly created <paramref name="survey"/> and returns the tracked instance.</summary>
    Task<Survey> CreateAsync(Survey survey, CancellationToken ct = default);

    /// <summary>Saves pending changes to <paramref name="survey"/> tracked by the current DbContext.</summary>
    Task UpdateAsync(Survey survey, CancellationToken ct = default);
}
