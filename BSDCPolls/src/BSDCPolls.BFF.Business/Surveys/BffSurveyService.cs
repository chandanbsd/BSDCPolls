using System.Net.Http.Headers;
using System.Net.Http.Json;
using BSDCPolls.Contracts.Enums;
using BSDCPolls.Contracts.Requests.Surveys;
using BSDCPolls.Contracts.Responses.Surveys;
using Microsoft.Extensions.Logging;

namespace BSDCPolls.BFF.Business.Surveys;

/// <summary>Forwards survey REST requests from the BFF to the internal backend API.</summary>
public sealed class BffSurveyService : IBffSurveyService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<BffSurveyService> _logger;

    /// <summary>Initialises the service with the HTTP client factory and logger.</summary>
    public BffSurveyService(IHttpClientFactory httpClientFactory, ILogger<BffSurveyService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<SurveyDetailResponse> CreateAsync(CreateSurveyRequest request, string bearerToken, CancellationToken ct = default)
    {
        var client = CreateAuthenticatedClient(bearerToken);
        var response = await client.PostAsJsonAsync("/api/internal/surveys", request, ct);
        await EnsureSuccessAsync(response, "Create survey", ct);
        return (await response.Content.ReadFromJsonAsync<SurveyDetailResponse>(ct))!;
    }

    /// <inheritdoc />
    public async Task<SurveyDetailResponse> GetByUidAsync(Guid surveyUid, string bearerToken, CancellationToken ct = default)
    {
        var client = CreateAuthenticatedClient(bearerToken);
        var response = await client.GetAsync($"/api/internal/surveys/{surveyUid}", ct);
        await EnsureSuccessAsync(response, "Get survey", ct);
        return (await response.Content.ReadFromJsonAsync<SurveyDetailResponse>(ct))!;
    }

    /// <inheritdoc />
    public async Task<SurveyFeedResponse> GetFeedAsync(SurveyStatus? status, int page, int pageSize, string bearerToken, CancellationToken ct = default)
    {
        var query = $"/api/internal/surveys?page={page}&pageSize={pageSize}";
        if (status.HasValue)
        {
            query += $"&status={status.Value}";
        }

        var client = CreateAuthenticatedClient(bearerToken);
        var response = await client.GetAsync(query, ct);
        await EnsureSuccessAsync(response, "Get survey feed", ct);
        return (await response.Content.ReadFromJsonAsync<SurveyFeedResponse>(ct))!;
    }

    /// <inheritdoc />
    public async Task<SurveyDetailResponse> ChangeStatusAsync(Guid surveyUid, ChangeSurveyStatusRequest request, string bearerToken, CancellationToken ct = default)
    {
        var client = CreateAuthenticatedClient(bearerToken);
        var response = await client.PatchAsJsonAsync($"/api/internal/surveys/{surveyUid}/status", request, ct);
        await EnsureSuccessAsync(response, "Change survey status", ct);
        return (await response.Content.ReadFromJsonAsync<SurveyDetailResponse>(ct))!;
    }

    /// <inheritdoc />
    public async Task<SurveyDetailResponse> UpdateQuestionsAsync(Guid surveyUid, UpdateSurveyQuestionsRequest request, string bearerToken, CancellationToken ct = default)
    {
        var client = CreateAuthenticatedClient(bearerToken);
        var response = await client.PutAsJsonAsync($"/api/internal/surveys/{surveyUid}/questions", request, ct);
        await EnsureSuccessAsync(response, "Update survey questions", ct);
        return (await response.Content.ReadFromJsonAsync<SurveyDetailResponse>(ct))!;
    }

    /// <inheritdoc />
    public async Task<SurveyResponseStatusResponse> SaveResponseAsync(Guid surveyUid, SaveSurveyResponseRequest request, string bearerToken, CancellationToken ct = default)
    {
        var client = CreateAuthenticatedClient(bearerToken);
        var response = await client.PostAsJsonAsync($"/api/internal/surveys/{surveyUid}/responses", request, ct);
        await EnsureSuccessAsync(response, "Save survey response", ct);
        return (await response.Content.ReadFromJsonAsync<SurveyResponseStatusResponse>(ct))!;
    }

    /// <inheritdoc />
    public async Task<SurveyDocumentResponse> UploadDocumentAsync(
        Guid surveyUid,
        Guid responseUid,
        Stream fileStream,
        string fileName,
        string contentType,
        long fileSize,
        Guid questionUid,
        string bearerToken,
        CancellationToken ct = default)
    {
        var client = CreateAuthenticatedClient(bearerToken);

        using var content = new MultipartFormDataContent();
        var fileContent = new StreamContent(fileStream);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue(contentType);
        content.Add(fileContent, "file", fileName);
        content.Add(new StringContent(questionUid.ToString()), "questionUid");

        var response = await client.PostAsync(
            $"/api/internal/surveys/{surveyUid}/responses/{responseUid}/documents",
            content,
            ct);
        await EnsureSuccessAsync(response, "Upload survey document", ct);
        return (await response.Content.ReadFromJsonAsync<SurveyDocumentResponse>(ct))!;
    }

    /// <inheritdoc />
    public async Task<SurveyResultsResponse> GetResultsAsync(Guid surveyUid, string bearerToken, CancellationToken ct = default)
    {
        var client = CreateAuthenticatedClient(bearerToken);
        var response = await client.GetAsync($"/api/internal/surveys/{surveyUid}/results", ct);
        await EnsureSuccessAsync(response, "Get survey results", ct);
        return (await response.Content.ReadFromJsonAsync<SurveyResultsResponse>(ct))!;
    }

    private HttpClient CreateAuthenticatedClient(string bearerToken)
    {
        var client = _httpClientFactory.CreateClient("InternalApi");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);
        return client;
    }

    private async Task EnsureSuccessAsync(HttpResponseMessage response, string operation, CancellationToken ct)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        var body = await response.Content.ReadAsStringAsync(ct);
        _logger.LogWarning("{Operation} failed ({Status}): {Body}", operation, (int)response.StatusCode, body);
        response.EnsureSuccessStatusCode();
    }
}
