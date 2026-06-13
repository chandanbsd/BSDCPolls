using FluentValidation;

namespace BSDCPolls.Contracts.Requests.Polls;

/// <summary>Validates <see cref="AddPollQuestionRequest"/>.</summary>
public sealed class AddPollQuestionRequestValidator : AbstractValidator<AddPollQuestionRequest>
{
    /// <summary>Initializes all validation rules.</summary>
    public AddPollQuestionRequestValidator()
    {
        RuleFor(x => x.Text).NotEmpty().MaximumLength(500);
        RuleFor(x => x.Options)
            .NotNull()
            .Must(opts => opts.Count >= 2).WithMessage("A question must have at least 2 answer options.")
            .Must(opts => opts.Count <= 10).WithMessage("A question may have at most 10 answer options.");
        RuleForEach(x => x.Options).ChildRules(opt =>
        {
            opt.RuleFor(o => o.Text).NotEmpty().MaximumLength(200);
            opt.RuleFor(o => o.OrderIndex).GreaterThanOrEqualTo(0);
        });
        RuleFor(x => x.PushImmediately).NotNull();
    }
}
