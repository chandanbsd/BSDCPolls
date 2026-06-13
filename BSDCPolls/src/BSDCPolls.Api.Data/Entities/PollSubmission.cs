namespace BSDCPolls.Api.Data.Entities;

/// <summary>
/// Records one participant's answer to one <see cref="PollQuestion"/>.
/// The unique constraint (PollQuestionId, RespondentId) enforces one vote per participant.
/// </summary>
public class PollSubmission : AuditableEntity
{
    /// <summary>FK to the question being answered.</summary>
    public int PollQuestionId { get; private set; }

    /// <summary>Navigation property to the parent question.</summary>
    public virtual PollQuestion PollQuestion { get; private set; } = null!;

    /// <summary>FK to the chosen answer option.</summary>
    public int SelectedOptionId { get; private set; }

    /// <summary>Navigation property to the selected option.</summary>
    public virtual PollAnswerOption SelectedOption { get; private set; } = null!;

    /// <summary>FK to the participant who submitted this vote.</summary>
    public int RespondentId { get; private set; }

    /// <summary>Navigation property to the respondent.</summary>
    public virtual ApplicationUser Respondent { get; private set; } = null!;

    /// <summary>EF Core proxy constructor.</summary>
    protected PollSubmission() { }

    private PollSubmission(int pollQuestionId, int selectedOptionId, int respondentId)
    {
        InitialiseIdentity(Guid.NewGuid());
        PollQuestionId = pollQuestionId;
        SelectedOptionId = selectedOptionId;
        RespondentId = respondentId;
    }

    /// <summary>Records a participant's vote on a question.</summary>
    /// <param name="pollQuestionId">Internal ID of the question being answered.</param>
    /// <param name="selectedOptionId">Internal ID of the chosen answer option.</param>
    /// <param name="respondentId">Internal ID of the participant casting the vote.</param>
    /// <returns>A new <see cref="PollSubmission"/> ready for persistence.</returns>
    public static PollSubmission Create(int pollQuestionId, int selectedOptionId, int respondentId)
    {
        return new PollSubmission(pollQuestionId, selectedOptionId, respondentId);
    }
}
