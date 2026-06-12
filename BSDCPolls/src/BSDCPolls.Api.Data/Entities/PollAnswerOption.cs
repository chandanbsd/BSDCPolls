using System.ComponentModel.DataAnnotations;

namespace BSDCPolls.Api.Data.Entities;

/// <summary>
/// One selectable choice within a <see cref="PollQuestion"/>.
/// </summary>
public class PollAnswerOption : AuditableEntity
{
    /// <summary>FK to the parent question.</summary>
    public int PollQuestionId { get; private set; }

    /// <summary>Navigation property to the parent question.</summary>
    public virtual PollQuestion PollQuestion { get; private set; } = null!;

    /// <summary>Display text for this answer choice.</summary>
    [Required]
    [MaxLength(200)]
    public string Text { get; private set; } = string.Empty;

    /// <summary>Display order within the question.</summary>
    public int OrderIndex { get; private set; }

    /// <summary>Submissions that selected this option.</summary>
    public virtual ICollection<PollSubmission> Submissions { get; private set; } = new List<PollSubmission>();

    /// <summary>EF Core proxy constructor.</summary>
    protected PollAnswerOption()
    {
    }

    private PollAnswerOption(int pollQuestionId, string text, int orderIndex)
    {
        InitialiseIdentity(Guid.NewGuid());
        PollQuestionId = pollQuestionId;
        Text = text;
        OrderIndex = orderIndex;
    }

    /// <summary>Creates a new answer option for a question.</summary>
    /// <param name="pollQuestionId">Internal ID of the parent question.</param>
    /// <param name="text">Display text for this choice.</param>
    /// <param name="orderIndex">Display order within the question.</param>
    /// <returns>A new <see cref="PollAnswerOption"/> ready for persistence.</returns>
    public static PollAnswerOption Create(int pollQuestionId, string text, int orderIndex)
    {
        if (string.IsNullOrWhiteSpace(text))
            throw new ArgumentException("Option text must not be empty.", nameof(text));

        return new PollAnswerOption(pollQuestionId, text, orderIndex);
    }
}
