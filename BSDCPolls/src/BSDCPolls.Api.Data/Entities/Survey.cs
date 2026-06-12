using System.ComponentModel.DataAnnotations;
using BSDCPolls.Contracts.Documents;
using BSDCPolls.Contracts.Enums;

namespace BSDCPolls.Api.Data.Entities;

/// <summary>
/// A structured questionnaire with conditional branching stored as a JSONB document.
/// </summary>
public class Survey : AuditableEntity
{
    /// <summary>Display title shown to respondents.</summary>
    [Required]
    [MaxLength(200)]
    public string Title { get; private set; } = string.Empty;

    /// <summary>When true the survey appears in the public home feed; otherwise invite-only.</summary>
    public bool IsPublic { get; private set; }

    /// <summary>Lifecycle state: Draft → Published → Closed (no reversal).</summary>
    public SurveyStatus Status { get; private set; }

    /// <summary>
    /// Full conditional question tree stored as a JSONB document.
    /// Mapped via <c>SurveyQuestionTreeConverter</c> value converter.
    /// </summary>
    public SurveyQuestionTreeDocument QuestionTree { get; private set; } = new(new List<SurveyQuestionNode>());

    /// <summary>FK to the creator. Also the user who controls publication state.</summary>
    public int CreatorId { get; private set; }

    /// <summary>Navigation property to the creating user.</summary>
    public virtual ApplicationUser Creator { get; private set; } = null!;

    /// <summary>Respondent answer submissions for this survey.</summary>
    public virtual ICollection<SurveyResponse> Responses { get; private set; } = new List<SurveyResponse>();

    /// <summary>Directed invitations to this survey (for invite-only surveys).</summary>
    public virtual ICollection<Invitation> Invitations { get; private set; } = new List<Invitation>();

    /// <summary>EF Core proxy constructor.</summary>
    protected Survey()
    {
    }

    private Survey(string title, bool isPublic, int creatorId, SurveyQuestionTreeDocument questionTree)
    {
        InitialiseIdentity(Guid.NewGuid());
        Title = title;
        IsPublic = isPublic;
        Status = SurveyStatus.Draft;
        CreatorId = creatorId;
        QuestionTree = questionTree;
    }

    /// <summary>Creates a new survey in <see cref="SurveyStatus.Draft"/> state.</summary>
    /// <param name="title">Display title.</param>
    /// <param name="isPublic">Whether the survey appears in the public feed.</param>
    /// <param name="creatorId">Internal ID of the creator.</param>
    /// <param name="questionTree">Initial question tree (may be empty draft).</param>
    /// <returns>A new <see cref="Survey"/> ready for persistence.</returns>
    public static Survey Create(
        string title,
        bool isPublic,
        int creatorId,
        SurveyQuestionTreeDocument questionTree)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title must not be empty.", nameof(title));

        return new Survey(title, isPublic, creatorId, questionTree);
    }

    /// <summary>Updates the survey's question tree while still in Draft state.</summary>
    /// <param name="questionTree">Revised question tree.</param>
    public void UpdateQuestionTree(SurveyQuestionTreeDocument questionTree)
    {
        if (Status != SurveyStatus.Draft)
            throw new InvalidOperationException("Question tree can only be updated while the survey is in Draft state.");

        QuestionTree = questionTree;
    }

    /// <summary>Publishes the survey, making it accessible to respondents. Only valid from Draft.</summary>
    public void Publish()
    {
        if (Status != SurveyStatus.Draft)
            throw new InvalidOperationException($"Cannot publish a survey with status {Status}.");

        Status = SurveyStatus.Published;
    }

    /// <summary>Closes the survey, preventing new responses. Only valid from Published.</summary>
    public void Close()
    {
        if (Status != SurveyStatus.Published)
            throw new InvalidOperationException($"Cannot close a survey with status {Status}.");

        Status = SurveyStatus.Closed;
    }
}
