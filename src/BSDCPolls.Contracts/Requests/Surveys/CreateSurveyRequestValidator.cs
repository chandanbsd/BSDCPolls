using FluentValidation;

namespace BSDCPolls.Contracts.Requests.Surveys;

/// <summary>Validates <see cref="CreateSurveyRequest"/>.</summary>
public sealed class CreateSurveyRequestValidator : AbstractValidator<CreateSurveyRequest>
{
    /// <summary>Initializes all validation rules.</summary>
    public CreateSurveyRequestValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.IsPublic).NotNull();
        RuleFor(x => x.QuestionTree)
            .NotNull()
            .SetValidator(new SurveyQuestionTreeDocumentValidator());
    }
}
