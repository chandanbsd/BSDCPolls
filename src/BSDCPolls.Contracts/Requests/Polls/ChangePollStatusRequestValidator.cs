using BSDCPolls.Contracts.Enums;
using FluentValidation;

namespace BSDCPolls.Contracts.Requests.Polls;

/// <summary>Validates <see cref="ChangePollStatusRequest"/>.</summary>
public sealed class ChangePollStatusRequestValidator : AbstractValidator<ChangePollStatusRequest>
{
    /// <summary>Initializes all validation rules.</summary>
    public ChangePollStatusRequestValidator()
    {
        RuleFor(x => x.Status)
            .IsInEnum()
            .Must(s => s == PollStatus.Active || s == PollStatus.Closed)
            .WithMessage(
                "Status must be Active or Closed (Draft is the initial state; use create endpoint)."
            );
    }
}
