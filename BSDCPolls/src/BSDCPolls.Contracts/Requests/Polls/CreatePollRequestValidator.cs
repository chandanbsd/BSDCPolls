using FluentValidation;

namespace BSDCPolls.Contracts.Requests.Polls;

/// <summary>Validates <see cref="CreatePollRequest"/>.</summary>
public sealed class CreatePollRequestValidator : AbstractValidator<CreatePollRequest>
{
    /// <summary>Initializes all validation rules.</summary>
    public CreatePollRequestValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.IsPublic).NotNull();
    }
}
