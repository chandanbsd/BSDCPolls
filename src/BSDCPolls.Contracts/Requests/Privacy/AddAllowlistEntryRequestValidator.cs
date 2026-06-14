using FluentValidation;

namespace BSDCPolls.Contracts.Requests.Privacy;

/// <summary>Validates <see cref="AddAllowlistEntryRequest"/>.</summary>
public sealed class AddAllowlistEntryRequestValidator : AbstractValidator<AddAllowlistEntryRequest>
{
    /// <summary>Initialises the validator with allowlist entry rules.</summary>
    public AddAllowlistEntryRequestValidator()
    {
        RuleFor(x => x.Username).NotEmpty().MaximumLength(60);
    }
}
