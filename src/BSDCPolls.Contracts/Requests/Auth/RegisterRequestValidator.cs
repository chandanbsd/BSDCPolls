using FluentValidation;

namespace BSDCPolls.Contracts.Requests.Auth;

/// <summary>Validates password strength rules for <see cref="RegisterRequest"/>.</summary>
public sealed class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    /// <summary>Initializes all password strength validation rules.</summary>
    public RegisterRequestValidator()
    {
        RuleFor(x => x.Password)
            .NotEmpty()
            .MinimumLength(12)
            .Matches(@"[A-Z]")
            .WithMessage("Must contain at least one uppercase letter.")
            .Matches(@"[a-z]")
            .WithMessage("Must contain at least one lowercase letter.")
            .Matches(@"[0-9]")
            .WithMessage("Must contain at least one digit.")
            .Matches(@"[^a-zA-Z0-9]")
            .WithMessage("Must contain at least one special character.");
    }
}
