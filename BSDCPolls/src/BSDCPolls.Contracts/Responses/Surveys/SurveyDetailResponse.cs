using BSDCPolls.Contracts.Documents;
using BSDCPolls.Contracts.Enums;

namespace BSDCPolls.Contracts.Responses.Surveys;

/// <summary>Full survey details including the conditional question tree.</summary>
/// <param name="SurveyUid">Public GUID identifier.</param>
/// <param name="Title">Survey title.</param>
/// <param name="IsPublic">Whether the survey is visible to all users.</param>
/// <param name="Status">Current survey lifecycle status.</param>
/// <param name="QuestionTree">The full conditional question tree.</param>
/// <param name="CreatedOn">UTC creation timestamp.</param>
/// <param name="IsCreator">True when the requesting user owns this survey.</param>
public sealed record SurveyDetailResponse(
    Guid SurveyUid,
    string Title,
    bool IsPublic,
    SurveyStatus Status,
    SurveyQuestionTreeDocument QuestionTree,
    DateTime CreatedOn,
    bool IsCreator);
