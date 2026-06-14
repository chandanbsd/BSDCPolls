using FluentValidation;

namespace BSDCPolls.Contracts.Requests.Privacy;

/// <summary>Validates <see cref="UpdatePrivacySettingsRequest"/>.</summary>
public sealed class UpdatePrivacySettingsRequestValidator
    : AbstractValidator<UpdatePrivacySettingsRequest>
{
    /// <summary>Initialises the validator with privacy settings rules.</summary>
    public UpdatePrivacySettingsRequestValidator()
    {
        RuleFor(x => x.InvitePermission).IsInEnum();
    }
}
