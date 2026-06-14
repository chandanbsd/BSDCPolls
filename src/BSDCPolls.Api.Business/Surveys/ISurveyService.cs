using BSDCPolls.Contracts.Enums;
using BSDCPolls.Contracts.Requests.Surveys;
using BSDCPolls.Contracts.Responses.Surveys;

namespace BSDCPolls.Api.Business.Surveys;

/// <summary>Domain service for survey lifecycle, question tree management, and response collection.</summary>
public interface ISurveyService
{
    /// <summary>Creates a new survey in Draft status owned by <paramref name="creatorId"/>.</summary>
    Task<SurveyDetailResponse> CreateAsync(
        CreateSurveyRequest request,
        int creatorId,
        CancellationToken ct = default
    );

    /// <summary>Returns full survey details if the requesting user is authorized to view it.</summary>
    Task<SurveyDetailResponse> GetByUidAsync(
        Guid uid,
        int requestingUserId,
        CancellationToken ct = default
    );

    /// <summary>Returns a paginated feed of surveys visible to <paramref name="userId"/>.</summary>
    Task<SurveyFeedResponse> GetFeedAsync(
        int userId,
        bool showPublic,
        SurveyStatus? status,
        int page,
        int pageSize,
        CancellationToken ct = default
    );

    /// <summary>Changes the survey status (Published or Closed).</summary>
    Task<SurveyDetailResponse> ChangeStatusAsync(
        Guid surveyUid,
        SurveyStatus newStatus,
        int creatorId,
        CancellationToken ct = default
    );

    /// <summary>Replaces the question tree on a Draft survey.</summary>
    Task<SurveyDetailResponse> UpdateQuestionsAsync(
        Guid surveyUid,
        UpdateSurveyQuestionsRequest request,
        int creatorId,
        CancellationToken ct = default
    );

    /// <summary>
    /// Saves progress or submits a respondent's answers.
    /// Creates the response record if it doesn't exist yet.
    /// </summary>
    Task<SurveyResponseStatusResponse> SaveResponseAsync(
        Guid surveyUid,
        SaveSurveyResponseRequest request,
        int respondentId,
        CancellationToken ct = default
    );

    /// <summary>
    /// Stores an uploaded PDF for a document-upload question.
    /// Validates that the content type is <c>application/pdf</c> and size is within the 10 MB limit.
    /// </summary>
    Task<SurveyDocumentResponse> UploadDocumentAsync(
        Guid surveyUid,
        Guid responseUid,
        Stream pdfStream,
        string fileName,
        long fileSize,
        Guid questionUid,
        int respondentId,
        CancellationToken ct = default
    );

    /// <summary>Returns aggregated survey results. Only accessible to the creator.</summary>
    Task<SurveyResultsResponse> GetResultsAsync(
        Guid surveyUid,
        int requestingUserId,
        CancellationToken ct = default
    );
}
