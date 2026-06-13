using BSDCPolls.Contracts.Documents;
using BSDCPolls.Contracts.Enums;
using FluentValidation;

namespace BSDCPolls.Contracts.Requests.Surveys;

/// <summary>Validates a single <see cref="SurveyQuestionNode"/> including recursive branch validation.</summary>
public sealed class SurveyQuestionNodeValidator : AbstractValidator<SurveyQuestionNode>
{
    /// <summary>Initializes all validation rules including cross-field and recursive branch rules.</summary>
    public SurveyQuestionNodeValidator()
    {
        RuleFor(x => x.Text).NotEmpty().MaximumLength(500);
        RuleFor(x => x.AnswerType).IsInEnum();

        RuleFor(x => x.Choices)
            .Must(
                (node, choices) =>
                    node.AnswerType != SurveyAnswerType.MultipleChoice ||
                    (choices != null && choices.Count >= 2))
            .WithMessage("Multiple-choice questions must have at least 2 choices.");

        RuleFor(x => x.Choices)
            .Must(
                (node, choices) =>
                    node.AnswerType == SurveyAnswerType.MultipleChoice ||
                    choices == null ||
                    choices.Count == 0)
            .WithMessage("Non-multiple-choice questions must not define answer choices.");

        RuleFor(x => x.Branches)
            .Must(
                (node, branches) =>
                    node.AnswerType != SurveyAnswerType.DocumentUpload ||
                    branches == null ||
                    branches.Count == 0)
            .WithMessage("Document-upload questions may not have conditional branches.");

        RuleForEach(x => x.Branches).ChildRules(branch =>
        {
            branch.RuleFor(b => b.ParentChoiceUid).NotEmpty();
            branch.RuleFor(b => b.Questions).NotNull().NotEmpty();
            branch.RuleForEach(b => b.Questions).SetValidator(new SurveyQuestionNodeValidator());
        });
    }
}
