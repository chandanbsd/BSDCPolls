using BSDCPolls.Contracts.Enums;

namespace BSDCPolls.Contracts.Documents;

/// <summary>
/// A single question node in the survey tree. May contain conditional branches.
/// </summary>
/// <param name="Uid">Stable identifier for this question within the tree.</param>
/// <param name="Text">The question text displayed to respondents.</param>
/// <param name="AnswerType">The type of answer expected from the respondent.</param>
/// <param name="IsRequired">Whether respondents must answer this question before proceeding.</param>
/// <param name="Choices">Available choices; non-null only when AnswerType is MultipleChoice.</param>
/// <param name="Branches">Conditional follow-up branches; keyed by the parent choice that triggers each branch.</param>
public sealed record SurveyQuestionNode(
    Guid Uid,
    string Text,
    SurveyAnswerType AnswerType,
    bool IsRequired,
    IReadOnlyList<SurveyChoiceOption>? Choices,
    IReadOnlyList<SurveyConditionalBranch>? Branches
);
