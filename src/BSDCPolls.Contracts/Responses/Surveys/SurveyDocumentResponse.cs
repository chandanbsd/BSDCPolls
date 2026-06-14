namespace BSDCPolls.Contracts.Responses.Surveys;

/// <summary>Metadata about an uploaded PDF document for a document-upload survey question.</summary>
/// <param name="DocumentUid">Public GUID of the stored document; used in the response answers.</param>
/// <param name="FileName">Original filename from the browser upload.</param>
/// <param name="FileSizeBytes">File size in bytes.</param>
public sealed record SurveyDocumentResponse(Guid DocumentUid, string FileName, long FileSizeBytes);
