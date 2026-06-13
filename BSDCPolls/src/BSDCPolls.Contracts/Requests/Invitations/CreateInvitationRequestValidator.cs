using FluentValidation;

namespace BSDCPolls.Contracts.Requests.Invitations;

/// <summary>Validates <see cref="CreateInvitationRequest"/>.</summary>
public sealed class CreateInvitationRequestValidator : AbstractValidator<CreateInvitationRequest>
{
    /// <summary>Initializes all validation rules.</summary>
    public CreateInvitationRequestValidator()
    {
        RuleFor(x => x.TargetUsername).NotEmpty().MaximumLength(60);
    }
}
