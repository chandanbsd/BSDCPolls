using BSDCPolls.Contracts.Enums;

namespace BSDCPolls.Contracts.Responses.Surveys;

/// <summary>Aggregated results for a single survey question.</summary>
/// <param name="QuestionUid">Public GUID of the question node.</param>
/// <param name="Text">Question text.</param>
/// <param name="AnswerType">The answer type for this question.</param>
/// <param name="ResponseCount">Number of respondents who answered this question.</param>
/// <param name="ChoiceTallies">Vote counts per choice; only populated for MultipleChoice questions.</param>
/// <param name="TextAnswers">Collected text responses; only populated for ShortText/LongText questions.</param>
/// <param name="DocumentCount">Number of uploaded documents; only populated for DocumentUpload questions.</param>
public sealed record SurveyQuestionSummary(
    Guid QuestionUid,
    string Text,
    SurveyAnswerType AnswerType,
    int ResponseCount,
    IReadOnlyList<SurveyChoiceTally> ChoiceTallies,
    IReadOnlyList<string> TextAnswers,
    int DocumentCount);
