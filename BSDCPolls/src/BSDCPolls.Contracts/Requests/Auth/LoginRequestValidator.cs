using FluentValidation;

namespace BSDCPolls.Contracts.Requests.Auth;

/// <summary>Validates credentials are non-empty for <see cref="LoginRequest"/>.</summary>
public sealed class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    /// <summary>Initializes validation rules for username and password fields.</summary>
    public LoginRequestValidator()
    {
        RuleFor(x => x.Username).NotEmpty().MaximumLength(60);
        RuleFor(x => x.Password).NotEmpty();
    }
}
