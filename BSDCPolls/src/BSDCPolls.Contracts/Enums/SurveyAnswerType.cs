namespace BSDCPolls.Contracts.Enums;

/// <summary>Answer type for a survey question.</summary>
public enum SurveyAnswerType
{
    /// <summary>Respondent selects one option from a list.</summary>
    MultipleChoice = 0,

    /// <summary>Respondent enters a single-word text answer.</summary>
    ShortText = 1,

    /// <summary>Respondent enters a multi-word text answer.</summary>
    LongText = 2,

    /// <summary>Respondent uploads a PDF document.</summary>
    DocumentUpload = 3,
}
