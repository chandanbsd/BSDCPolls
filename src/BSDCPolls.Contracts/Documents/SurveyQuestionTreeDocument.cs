namespace BSDCPolls.Contracts.Documents;

/// <summary>
/// Root document for a survey's entire question tree.
/// Stored as a JSONB column in the Survey entity.
/// </summary>
/// <param name="Questions">Top-level questions in the survey.</param>
public sealed record SurveyQuestionTreeDocument(IReadOnlyList<SurveyQuestionNode> Questions);
