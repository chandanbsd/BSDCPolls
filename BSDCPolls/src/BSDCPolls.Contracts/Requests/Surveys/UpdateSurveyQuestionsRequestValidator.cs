using FluentValidation;

namespace BSDCPolls.Contracts.Requests.Surveys;

/// <summary>Validates <see cref="UpdateSurveyQuestionsRequest"/>.</summary>
public sealed class UpdateSurveyQuestionsRequestValidator : AbstractValidator<UpdateSurveyQuestionsRequest>
{
    /// <summary>Initializes all validation rules.</summary>
    public UpdateSurveyQuestionsRequestValidator()
    {
        RuleFor(x => x.QuestionTree).NotNull().SetValidator(new SurveyQuestionTreeDocumentValidator());
    }
}
