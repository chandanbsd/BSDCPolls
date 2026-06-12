using BSDCPolls.Contracts.Documents;

namespace BSDCPolls.Api.Data.Entities;

/// <summary>
/// One respondent's complete (or in-progress) answer set for a survey.
/// A unique constraint on (SurveyId, RespondentId) enforces one response per user per survey.
/// </summary>
public class SurveyResponse : AuditableEntity
{
    /// <summary>FK to the parent survey.</summary>
    public int SurveyId { get; private set; }

    /// <summary>Navigation property to the parent survey.</summary>
    public virtual Survey Survey { get; private set; } = null!;

    /// <summary>FK to the respondent.</summary>
    public int RespondentId { get; private set; }

    /// <summary>Navigation property to the respondent.</summary>
    public virtual ApplicationUser Respondent { get; private set; } = null!;

    /// <summary>
    /// Answered question UIDs with their values, stored as a JSONB document.
    /// Mapped via <c>SurveyAnswersConverter</c> value converter.
    /// </summary>
    public SurveyAnswersDocument AnswersJson { get; private set; } = new(new List<SurveyAnswerEntry>());

    /// <summary>False while the respondent is still filling in the survey; true after explicit submission.</summary>
    public bool IsComplete { get; private set; }

    /// <summary>UTC time the response was explicitly submitted. <c>null</c> until submitted.</summary>
    public DateTime? SubmittedAt { get; private set; }

    /// <summary>PDF files uploaded as answers to document-upload questions.</summary>
    public virtual ICollection<SurveyDocument> Documents { get; private set; } = new List<SurveyDocument>();

    /// <summary>EF Core proxy constructor.</summary>
    protected SurveyResponse()
    {
    }

    private SurveyResponse(int surveyId, int respondentId)
    {
        InitialiseIdentity(Guid.NewGuid());
        SurveyId = surveyId;
        RespondentId = respondentId;
        IsComplete = false;
    }

    /// <summary>Starts a new in-progress survey response.</summary>
    /// <param name="surveyId">Internal ID of the parent survey.</param>
    /// <param name="respondentId">Internal ID of the responding user.</param>
    /// <returns>A new <see cref="SurveyResponse"/> ready for persistence.</returns>
    public static SurveyResponse Create(int surveyId, int respondentId)
    {
        return new SurveyResponse(surveyId, respondentId);
    }

    /// <summary>Saves the current state of the respondent's answers without submitting.</summary>
    /// <param name="answers">Updated answers document.</param>
    public void SaveProgress(SurveyAnswersDocument answers)
    {
        if (IsComplete)
            throw new InvalidOperationException("Cannot update a completed survey response.");

        AnswersJson = answers;
    }

    /// <summary>Finalises the response. Sets IsComplete and SubmittedAt. Irreversible.</summary>
    /// <param name="answers">Final answers at the time of submission.</param>
    /// <param name="submittedAt">UTC timestamp of submission.</param>
    public void Submit(SurveyAnswersDocument answers, DateTime submittedAt)
    {
        if (IsComplete)
            throw new InvalidOperationException("Response has already been submitted.");

        AnswersJson = answers;
        IsComplete = true;
        SubmittedAt = submittedAt;
    }
}
