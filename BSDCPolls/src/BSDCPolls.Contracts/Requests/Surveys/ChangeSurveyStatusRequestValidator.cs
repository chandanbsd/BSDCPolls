using BSDCPolls.Contracts.Enums;
using FluentValidation;

namespace BSDCPolls.Contracts.Requests.Surveys;

/// <summary>Validates <see cref="ChangeSurveyStatusRequest"/>.</summary>
public sealed class ChangeSurveyStatusRequestValidator : AbstractValidator<ChangeSurveyStatusRequest>
{
    /// <summary>Initializes all validation rules.</summary>
    public ChangeSurveyStatusRequestValidator()
    {
        RuleFor(x => x.Status)
            .IsInEnum()
            .Must(s => s == SurveyStatus.Published || s == SurveyStatus.Closed)
            .WithMessage("Status must be Published or Closed (Draft is the initial state; use create endpoint).");
    }
}
