namespace BSDCPolls.Contracts.Documents;

/// <summary>
/// Root document storing all of a respondent's answers to a survey.
/// Stored as a JSONB column in the SurveyResponse entity.
/// </summary>
/// <param name="Answers">All answered question entries in this response.</param>
public sealed record SurveyAnswersDocument(IReadOnlyList<SurveyAnswerEntry> Answers);
