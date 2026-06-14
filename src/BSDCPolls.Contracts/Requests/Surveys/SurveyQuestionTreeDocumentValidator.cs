using BSDCPolls.Contracts.Documents;
using FluentValidation;

namespace BSDCPolls.Contracts.Requests.Surveys;

/// <summary>Validates a <see cref="SurveyQuestionTreeDocument"/> including each top-level node.</summary>
public sealed class SurveyQuestionTreeDocumentValidator
    : AbstractValidator<SurveyQuestionTreeDocument>
{
    /// <summary>Initializes all validation rules.</summary>
    public SurveyQuestionTreeDocumentValidator()
    {
        RuleFor(x => x.Questions)
            .NotNull()
            .NotEmpty()
            .WithMessage("A survey must have at least one question.");

        RuleForEach(x => x.Questions).SetValidator(new SurveyQuestionNodeValidator());
    }
}
