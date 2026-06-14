namespace BSDCPolls.Contracts.Responses.Surveys;

/// <summary>Status of a respondent's survey response after a save-progress or submit call.</summary>
/// <param name="ResponseUid">Public GUID of the response record.</param>
/// <param name="IsComplete">True once the response has been explicitly submitted.</param>
/// <param name="SubmittedAt">UTC submission timestamp; null until submitted.</param>
public sealed record SurveyResponseStatusResponse(
    Guid ResponseUid,
    bool IsComplete,
    DateTime? SubmittedAt
);
