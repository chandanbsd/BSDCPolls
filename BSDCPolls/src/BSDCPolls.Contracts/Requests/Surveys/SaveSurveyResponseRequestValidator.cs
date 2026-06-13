using BSDCPolls.Contracts.Enums;
using FluentValidation;

namespace BSDCPolls.Contracts.Requests.Surveys;

/// <summary>Validates <see cref="SaveSurveyResponseRequest"/>.</summary>
public sealed class SaveSurveyResponseRequestValidator : AbstractValidator<SaveSurveyResponseRequest>
{
    /// <summary>Initializes all validation rules including cross-field answer constraints.</summary>
    public SaveSurveyResponseRequestValidator()
    {
        RuleFor(x => x.Answers).NotNull();

        RuleForEach(x => x.Answers).ChildRules(ans =>
        {
            ans.RuleFor(a => a.QuestionUid).NotEmpty();
            ans.RuleFor(a => a.AnswerType).IsInEnum();

            ans.RuleFor(a => a.SelectedChoiceUid)
                .Must(
                    (entry, uid) =>
                        entry.AnswerType != SurveyAnswerType.MultipleChoice || uid.HasValue)
                .WithMessage("Multiple-choice answers must include a selected choice.");

            ans.RuleFor(a => a.TextValue)
                .Must(
                    (entry, text) =>
                        (entry.AnswerType != SurveyAnswerType.ShortText &&
                         entry.AnswerType != SurveyAnswerType.LongText) ||
                        !string.IsNullOrWhiteSpace(text))
                .WithMessage("Text answers must include a non-empty text value.");

            ans.RuleFor(a => a.DocumentUid)
                .Must(
                    (entry, uid) =>
                        entry.AnswerType != SurveyAnswerType.DocumentUpload || uid.HasValue)
                .WithMessage("Document-upload answers must reference an uploaded document.");
        });
    }
}
