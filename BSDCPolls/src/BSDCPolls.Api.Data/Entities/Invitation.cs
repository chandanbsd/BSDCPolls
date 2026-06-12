using BSDCPolls.Contracts.Enums;

namespace BSDCPolls.Api.Data.Entities;

/// <summary>
/// A directed invitation from a creator to a specific user for a poll or survey.
/// Exactly one of <see cref="PollId"/> or <see cref="SurveyId"/> must be non-null.
/// </summary>
public class Invitation : AuditableEntity
{
    /// <summary>FK to the user who sent the invitation.</summary>
    public int InviterId { get; private set; }

    /// <summary>Navigation property to the inviting user.</summary>
    public virtual ApplicationUser Inviter { get; private set; } = null!;

    /// <summary>FK to the user who received the invitation.</summary>
    public int InviteeId { get; private set; }

    /// <summary>Navigation property to the invited user.</summary>
    public virtual ApplicationUser Invitee { get; private set; } = null!;

    /// <summary>FK to the poll being invited to. Null when this is a survey invitation.</summary>
    public int? PollId { get; private set; }

    /// <summary>Navigation property to the invited poll.</summary>
    public virtual Poll? Poll { get; private set; }

    /// <summary>FK to the survey being invited to. Null when this is a poll invitation.</summary>
    public int? SurveyId { get; private set; }

    /// <summary>Navigation property to the invited survey.</summary>
    public virtual Survey? Survey { get; private set; }

    /// <summary>Current state of the invitation.</summary>
    public InvitationStatus Status { get; private set; }

    /// <summary>The notification generated when this invitation was created.</summary>
    public virtual Notification? Notification { get; private set; }

    /// <summary>EF Core proxy constructor.</summary>
    protected Invitation()
    {
    }

    private Invitation(int inviterId, int inviteeId, int? pollId, int? surveyId)
    {
        InitialiseIdentity(Guid.NewGuid());
        InviterId = inviterId;
        InviteeId = inviteeId;
        PollId = pollId;
        SurveyId = surveyId;
        Status = InvitationStatus.Pending;
    }

    /// <summary>Creates a poll invitation.</summary>
    /// <param name="inviterId">Internal ID of the user sending the invitation.</param>
    /// <param name="inviteeId">Internal ID of the user being invited.</param>
    /// <param name="pollId">Internal ID of the poll to invite to.</param>
    /// <returns>A new <see cref="Invitation"/> in <see cref="InvitationStatus.Pending"/> state.</returns>
    public static Invitation CreateForPoll(int inviterId, int inviteeId, int pollId)
    {
        return new Invitation(inviterId, inviteeId, pollId, null);
    }

    /// <summary>Creates a survey invitation.</summary>
    /// <param name="inviterId">Internal ID of the user sending the invitation.</param>
    /// <param name="inviteeId">Internal ID of the user being invited.</param>
    /// <param name="surveyId">Internal ID of the survey to invite to.</param>
    /// <returns>A new <see cref="Invitation"/> in <see cref="InvitationStatus.Pending"/> state.</returns>
    public static Invitation CreateForSurvey(int inviterId, int inviteeId, int surveyId)
    {
        return new Invitation(inviterId, inviteeId, null, surveyId);
    }

    /// <summary>Marks the invitation as viewed by the invitee.</summary>
    public void MarkViewed()
    {
        if (Status == InvitationStatus.Pending)
            Status = InvitationStatus.Viewed;
    }

    /// <summary>Marks the invitation as declined by the invitee.</summary>
    public void Decline()
    {
        if (Status == InvitationStatus.Declined)
            throw new InvalidOperationException("Invitation has already been declined.");

        Status = InvitationStatus.Declined;
    }
}
