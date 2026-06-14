using BSDCPolls.Api.Data.Entities;

namespace BSDCPolls.Api.Data.Repositories;

/// <summary>Data-access contract for <see cref="SurveyResponse"/> persistence operations.</summary>
public interface ISurveyResponseRepository
{
    /// <summary>
    /// Returns the in-progress or completed response for the given survey and respondent,
    /// or <c>null</c> if the respondent has not started a response.
    /// </summary>
    /// <param name="surveyUid">Public GUID of the parent survey.</param>
    /// <param name="respondentId">Internal ID of the respondent.</param>
    /// <param name="ct">Cancellation token.</param>
    Task<SurveyResponse?> GetByRespondentAsync(
        Guid surveyUid,
        int respondentId,
        CancellationToken ct = default
    );

    /// <summary>
    /// Returns true if the respondent has already submitted a completed response for the survey.
    /// </summary>
    /// <param name="surveyUid">Public GUID of the parent survey.</param>
    /// <param name="respondentId">Internal ID of the respondent.</param>
    /// <param name="ct">Cancellation token.</param>
    Task<bool> HasCompletedAsync(Guid surveyUid, int respondentId, CancellationToken ct = default);

    /// <summary>Persists a new <paramref name="response"/> and returns the tracked instance.</summary>
    Task<SurveyResponse> CreateAsync(SurveyResponse response, CancellationToken ct = default);

    /// <summary>Saves pending changes to <paramref name="response"/> tracked by the current DbContext.</summary>
    Task UpdateAsync(SurveyResponse response, CancellationToken ct = default);
}
