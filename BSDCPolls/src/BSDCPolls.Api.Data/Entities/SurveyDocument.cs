using System.ComponentModel.DataAnnotations;

namespace BSDCPolls.Api.Data.Entities;

/// <summary>
/// A PDF file uploaded by a respondent as the answer to a document-upload survey question.
/// Raw bytes are stored as PostgreSQL bytea (≤ 10 MB; validated at BFF boundary).
/// </summary>
public class SurveyDocument : AuditableEntity
{
    /// <summary>FK to the parent survey response.</summary>
    public int SurveyResponseId { get; private set; }

    /// <summary>Navigation property to the parent response.</summary>
    public virtual SurveyResponse SurveyResponse { get; private set; } = null!;

    /// <summary>
    /// Identifies which question within the survey this document answers.
    /// Matches a <c>SurveyQuestionNode.Uid</c> in the parent survey's question tree.
    /// </summary>
    public Guid QuestionUid { get; private set; }

    /// <summary>Original filename from the browser upload.</summary>
    [Required]
    [MaxLength(255)]
    public string FileName { get; private set; } = string.Empty;

    /// <summary>File size in bytes. Validated ≤ 10,485,760 (10 MB) at the BFF boundary.</summary>
    public long FileSizeBytes { get; private set; }

    /// <summary>Raw PDF bytes stored as PostgreSQL bytea.</summary>
    [Required]
    public byte[] FileData { get; private set; } = Array.Empty<byte>();

    /// <summary>EF Core proxy constructor.</summary>
    protected SurveyDocument() { }

    private SurveyDocument(
        int surveyResponseId,
        Guid questionUid,
        string fileName,
        long fileSizeBytes,
        byte[] fileData
    )
    {
        InitialiseIdentity(Guid.NewGuid());
        SurveyResponseId = surveyResponseId;
        QuestionUid = questionUid;
        FileName = fileName;
        FileSizeBytes = fileSizeBytes;
        FileData = fileData;
    }

    /// <summary>Records an uploaded PDF document for a survey response.</summary>
    /// <param name="surveyResponseId">Internal ID of the parent survey response.</param>
    /// <param name="questionUid">GUID of the document-upload question this file answers.</param>
    /// <param name="fileName">Original filename from the browser.</param>
    /// <param name="fileSizeBytes">File size in bytes.</param>
    /// <param name="fileData">Raw PDF bytes.</param>
    /// <returns>A new <see cref="SurveyDocument"/> ready for persistence.</returns>
    public static SurveyDocument Create(
        int surveyResponseId,
        Guid questionUid,
        string fileName,
        long fileSizeBytes,
        byte[] fileData
    )
    {
        if (string.IsNullOrWhiteSpace(fileName))
            throw new ArgumentException("File name must not be empty.", nameof(fileName));
        if (fileData.Length == 0)
            throw new ArgumentException("File data must not be empty.", nameof(fileData));

        return new SurveyDocument(surveyResponseId, questionUid, fileName, fileSizeBytes, fileData);
    }
}
