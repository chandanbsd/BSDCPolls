using System.ComponentModel.DataAnnotations;

namespace BSDCPolls.Api.Data.Entities;

/// <summary>
/// A single multiple-choice question within a poll session, pushed live by the creator.
/// </summary>
public class PollQuestion : AuditableEntity
{
    /// <summary>FK to the parent poll.</summary>
    public int PollId { get; private set; }

    /// <summary>Navigation property to the parent poll.</summary>
    public virtual Poll Poll { get; private set; } = null!;

    /// <summary>The question text shown to participants.</summary>
    [Required]
    [MaxLength(500)]
    public string Text { get; private set; } = string.Empty;

    /// <summary>Sequence number controlling display order within the poll.</summary>
    public int OrderIndex { get; private set; }

    /// <summary>
    /// UTC time the question was pushed to participants.
    /// <c>null</c> while the question is staged but not yet active.
    /// </summary>
    public DateTime? PushedAt { get; private set; }

    /// <summary>Answer choices for this question.</summary>
    public virtual ICollection<PollAnswerOption> AnswerOptions { get; private set; } = new List<PollAnswerOption>();

    /// <summary>Participant submissions for this question.</summary>
    public virtual ICollection<PollSubmission> Submissions { get; private set; } = new List<PollSubmission>();

    /// <summary>EF Core proxy constructor.</summary>
    protected PollQuestion()
    {
    }

    private PollQuestion(int pollId, string text, int orderIndex)
    {
        InitialiseIdentity(Guid.NewGuid());
        PollId = pollId;
        Text = text;
        OrderIndex = orderIndex;
    }

    /// <summary>Creates a new question staged within the given poll.</summary>
    /// <param name="pollId">Internal ID of the parent poll.</param>
    /// <param name="text">Question text shown to participants.</param>
    /// <param name="orderIndex">Sequence number within the poll.</param>
    /// <returns>A new <see cref="PollQuestion"/> ready for persistence.</returns>
    public static PollQuestion Create(int pollId, string text, int orderIndex)
    {
        if (string.IsNullOrWhiteSpace(text))
            throw new ArgumentException("Question text must not be empty.", nameof(text));

        return new PollQuestion(pollId, text, orderIndex);
    }

    /// <summary>
    /// Records that the creator pushed this question to all participants.
    /// Sets <see cref="PushedAt"/> to the current UTC time.
    /// </summary>
    /// <param name="pushedAt">UTC timestamp of the push event.</param>
    public void MarkPushed(DateTime pushedAt)
    {
        if (PushedAt.HasValue)
            throw new InvalidOperationException("Question has already been pushed.");

        PushedAt = pushedAt;
    }
}
