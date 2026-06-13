using BSDCPolls.Contracts.Enums;

namespace BSDCPolls.Contracts.Responses.Surveys;

/// <summary>Summary of a single survey as it appears in the home feed.</summary>
/// <param name="SurveyUid">Public GUID identifier.</param>
/// <param name="Title">Survey title.</param>
/// <param name="IsPublic">Whether the survey is visible to all users.</param>
/// <param name="Status">Current survey lifecycle status.</param>
/// <param name="CreatorUsername">Username of the survey creator.</param>
/// <param name="QuestionCount">Total number of top-level questions.</param>
/// <param name="CreatedOn">UTC creation timestamp.</param>
/// <param name="InvitedAt">UTC time the current user was invited; null for public access.</param>
public sealed record SurveyFeedItem(
    Guid SurveyUid,
    string Title,
    bool IsPublic,
    SurveyStatus Status,
    string CreatorUsername,
    int QuestionCount,
    DateTime CreatedOn,
    DateTime? InvitedAt);
