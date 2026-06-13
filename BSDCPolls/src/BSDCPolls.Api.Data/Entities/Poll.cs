using System.ComponentModel.DataAnnotations;
using BSDCPolls.Contracts.Enums;

namespace BSDCPolls.Api.Data.Entities;

/// <summary>
/// A real-time live polling session created by one user and participated in by others.
/// </summary>
public class Poll : AuditableEntity
{
    /// <summary>Human-readable title for the poll session.</summary>
    [Required]
    [MaxLength(200)]
    public string Title { get; private set; } = string.Empty;

    /// <summary>When true the poll appears in the public home feed; otherwise invite-only.</summary>
    public bool IsPublic { get; private set; }

    /// <summary>Lifecycle state: Draft → Active → Closed (no reversal).</summary>
    public PollStatus Status { get; private set; }

    /// <summary>FK to the creator. Also the user who controls the live session.</summary>
    public int CreatorId { get; private set; }

    /// <summary>Navigation property to the creating user.</summary>
    public virtual ApplicationUser Creator { get; private set; } = null!;

    /// <summary>Questions pushed during the live session.</summary>
    public virtual ICollection<PollQuestion> Questions { get; private set; } =
        new List<PollQuestion>();

    /// <summary>Directed invitations to this poll (for invite-only polls).</summary>
    public virtual ICollection<Invitation> Invitations { get; private set; } =
        new List<Invitation>();

    /// <summary>EF Core proxy constructor.</summary>
    protected Poll() { }

    private Poll(string title, bool isPublic, int creatorId)
    {
        InitialiseIdentity(Guid.NewGuid());
        Title = title;
        IsPublic = isPublic;
        Status = PollStatus.Draft;
        CreatorId = creatorId;
    }

    /// <summary>Creates a new poll in <see cref="PollStatus.Draft"/> state.</summary>
    /// <param name="title">Display title for the poll session.</param>
    /// <param name="isPublic">Whether the poll appears in the public feed.</param>
    /// <param name="creatorId">Internal ID of the creator.</param>
    /// <returns>A new <see cref="Poll"/> ready for persistence.</returns>
    public static Poll Create(string title, bool isPublic, int creatorId)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title must not be empty.", nameof(title));

        return new Poll(title, isPublic, creatorId);
    }

    /// <summary>Activates the poll, beginning the live session. Only valid from Draft.</summary>
    public void Activate()
    {
        if (Status != PollStatus.Draft)
            throw new InvalidOperationException($"Cannot activate a poll with status {Status}.");

        Status = PollStatus.Active;
    }

    /// <summary>Closes the live session. Only valid from Active.</summary>
    public void Close()
    {
        if (Status != PollStatus.Active)
            throw new InvalidOperationException($"Cannot close a poll with status {Status}.");

        Status = PollStatus.Closed;
    }
}
