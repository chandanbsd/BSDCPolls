using FluentValidation;

namespace BSDCPolls.Contracts.Requests.Polls;

/// <summary>Validates <see cref="SubmitPollVoteRequest"/>.</summary>
public sealed class SubmitPollVoteRequestValidator : AbstractValidator<SubmitPollVoteRequest>
{
    /// <summary>Initializes all validation rules.</summary>
    public SubmitPollVoteRequestValidator()
    {
        RuleFor(x => x.QuestionUid).NotEmpty();
        RuleFor(x => x.SelectedOptionUid).NotEmpty();
    }
}
