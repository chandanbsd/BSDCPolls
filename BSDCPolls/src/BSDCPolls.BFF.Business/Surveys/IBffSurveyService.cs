using BSDCPolls.Contracts.Enums;
using BSDCPolls.Contracts.Requests.Surveys;
using BSDCPolls.Contracts.Responses.Surveys;

namespace BSDCPolls.BFF.Business.Surveys;

/// <summary>Forwards survey REST requests from the BFF to the internal backend API.</summary>
public interface IBffSurveyService
{
    /// <summary>Forwards a create-survey request to the internal API.</summary>
    Task<SurveyDetailResponse> CreateAsync(
        CreateSurveyRequest request,
        string bearerToken,
        CancellationToken ct = default
    );

    /// <summary>Forwards a get-survey-by-uid request to the internal API.</summary>
    Task<SurveyDetailResponse> GetByUidAsync(
        Guid surveyUid,
        string bearerToken,
        CancellationToken ct = default
    );

    /// <summary>Forwards a get-survey-feed request to the internal API.</summary>
    Task<SurveyFeedResponse> GetFeedAsync(
        SurveyStatus? status,
        int page,
        int pageSize,
        string bearerToken,
        CancellationToken ct = default
    );

    /// <summary>Forwards a change-status request to the internal API.</summary>
    Task<SurveyDetailResponse> ChangeStatusAsync(
        Guid surveyUid,
        ChangeSurveyStatusRequest request,
        string bearerToken,
        CancellationToken ct = default
    );

    /// <summary>Forwards an update-questions request to the internal API.</summary>
    Task<SurveyDetailResponse> UpdateQuestionsAsync(
        Guid surveyUid,
        UpdateSurveyQuestionsRequest request,
        string bearerToken,
        CancellationToken ct = default
    );

    /// <summary>Forwards a save-response request to the internal API.</summary>
    Task<SurveyResponseStatusResponse> SaveResponseAsync(
        Guid surveyUid,
        SaveSurveyResponseRequest request,
        string bearerToken,
        CancellationToken ct = default
    );

    /// <summary>
    /// Forwards a document upload to the internal API using multipart/form-data.
    /// </summary>
    Task<SurveyDocumentResponse> UploadDocumentAsync(
        Guid surveyUid,
        Guid responseUid,
        Stream fileStream,
        string fileName,
        string contentType,
        long fileSize,
        Guid questionUid,
        string bearerToken,
        CancellationToken ct = default
    );

    /// <summary>Forwards a get-results request to the internal API.</summary>
    Task<SurveyResultsResponse> GetResultsAsync(
        Guid surveyUid,
        string bearerToken,
        CancellationToken ct = default
    );
}
