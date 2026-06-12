using BSDCPolls.Contracts.Enums;

namespace BSDCPolls.Contracts.Documents;

/// <summary>
/// A single answer entry for one survey question.
/// Exactly one of <see cref="SelectedChoiceUid"/>, <see cref="TextValue"/>, or
/// <see cref="DocumentUid"/> will be non-null depending on the <see cref="AnswerType"/>.
/// </summary>
/// <param name="QuestionUid">The question this entry answers.</param>
/// <param name="AnswerType">The answer type of the question, for deserialization context.</param>
/// <param name="SelectedChoiceUid">The selected choice; non-null for MultipleChoice answers.</param>
/// <param name="TextValue">The text value; non-null for ShortText and LongText answers.</param>
/// <param name="DocumentUid">Reference to the uploaded SurveyDocument; non-null for DocumentUpload answers.</param>
public sealed record SurveyAnswerEntry(
    Guid QuestionUid,
    SurveyAnswerType AnswerType,
    Guid? SelectedChoiceUid,
    string? TextValue,
    Guid? DocumentUid);
