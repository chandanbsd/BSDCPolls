using BSDCPolls.Contracts.Enums;

namespace BSDCPolls.Contracts.Responses.Surveys;

/// <summary>Aggregated results for an entire survey (creator only).</summary>
/// <param name="SurveyUid">Public GUID of the survey.</param>
/// <param name="Title">Survey title.</param>
/// <param name="Status">Current survey lifecycle status.</param>
/// <param name="TotalResponses">Total number of started responses.</param>
/// <param name="CompleteResponses">Number of fully submitted responses.</param>
/// <param name="QuestionSummaries">Per-question aggregated answer summaries.</param>
public sealed record SurveyResultsResponse(
    Guid SurveyUid,
    string Title,
    SurveyStatus Status,
    int TotalResponses,
    int CompleteResponses,
    IReadOnlyList<SurveyQuestionSummary> QuestionSummaries);
