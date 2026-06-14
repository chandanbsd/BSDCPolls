using System.Net.Http.Headers;
using System.Net.Http.Json;
using BSDCPolls.Contracts.Enums;
using BSDCPolls.Contracts.Requests.Polls;
using BSDCPolls.Contracts.Responses.Polls;
using Microsoft.Extensions.Logging;

namespace BSDCPolls.BFF.Business.Polls;

/// <summary>Forwards poll REST requests from the BFF to the internal API.</summary>
public sealed class BffPollService : IBffPollService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<BffPollService> _logger;

    /// <summary>Initialises the service with the HTTP client factory and logger.</summary>
    public BffPollService(IHttpClientFactory httpClientFactory, ILogger<BffPollService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<PollDetailResponse> CreateAsync(
        CreatePollRequest request,
        string bearerToken,
        CancellationToken ct = default
    )
    {
        var client = CreateAuthenticatedClient(bearerToken);
        var response = await client.PostAsJsonAsync("/api/internal/polls", request, ct);
        await EnsureSuccessAsync(response, "Create poll", ct);
        return (await response.Content.ReadFromJsonAsync<PollDetailResponse>(ct))!;
    }

    /// <inheritdoc />
    public async Task<PollDetailResponse> GetByUidAsync(
        Guid pollUid,
        string bearerToken,
        CancellationToken ct = default
    )
    {
        var client = CreateAuthenticatedClient(bearerToken);
        var response = await client.GetAsync($"/api/internal/polls/{pollUid}", ct);
        await EnsureSuccessAsync(response, "Get poll", ct);
        return (await response.Content.ReadFromJsonAsync<PollDetailResponse>(ct))!;
    }

    /// <inheritdoc />
    public async Task<PollFeedResponse> GetFeedAsync(
        PollStatus? status,
        int page,
        int pageSize,
        string bearerToken,
        CancellationToken ct = default
    )
    {
        var query = $"/api/internal/polls?page={page}&pageSize={pageSize}";
        if (status.HasValue)
        {
            query += $"&status={status.Value}";
        }

        var client = CreateAuthenticatedClient(bearerToken);
        var response = await client.GetAsync(query, ct);
        await EnsureSuccessAsync(response, "Get poll feed", ct);
        return (await response.Content.ReadFromJsonAsync<PollFeedResponse>(ct))!;
    }

    /// <inheritdoc />
    public async Task<PollDetailResponse> ChangeStatusAsync(
        Guid pollUid,
        ChangePollStatusRequest request,
        string bearerToken,
        CancellationToken ct = default
    )
    {
        var client = CreateAuthenticatedClient(bearerToken);
        var response = await client.PatchAsJsonAsync(
            $"/api/internal/polls/{pollUid}/status",
            request,
            ct
        );
        await EnsureSuccessAsync(response, "Change poll status", ct);
        return (await response.Content.ReadFromJsonAsync<PollDetailResponse>(ct))!;
    }

    /// <inheritdoc />
    public async Task<PollQuestionResponse> AddQuestionAsync(
        Guid pollUid,
        AddPollQuestionRequest request,
        string bearerToken,
        CancellationToken ct = default
    )
    {
        var client = CreateAuthenticatedClient(bearerToken);
        var response = await client.PostAsJsonAsync(
            $"/api/internal/polls/{pollUid}/questions",
            request,
            ct
        );
        await EnsureSuccessAsync(response, "Add poll question", ct);
        return (await response.Content.ReadFromJsonAsync<PollQuestionResponse>(ct))!;
    }

    /// <inheritdoc />
    public async Task<PollQuestionResponse> PushQuestionAsync(
        Guid pollUid,
        Guid questionUid,
        string bearerToken,
        CancellationToken ct = default
    )
    {
        var client = CreateAuthenticatedClient(bearerToken);
        var response = await client.PostAsync(
            $"/api/internal/polls/{pollUid}/questions/{questionUid}/push",
            null,
            ct
        );
        await EnsureSuccessAsync(response, "Push poll question", ct);
        return (await response.Content.ReadFromJsonAsync<PollQuestionResponse>(ct))!;
    }

    /// <inheritdoc />
    public async Task<PollResultsResponse> GetResultsAsync(
        Guid pollUid,
        string bearerToken,
        CancellationToken ct = default
    )
    {
        var client = CreateAuthenticatedClient(bearerToken);
        var response = await client.GetAsync($"/api/internal/polls/{pollUid}/results", ct);
        await EnsureSuccessAsync(response, "Get poll results", ct);
        return (await response.Content.ReadFromJsonAsync<PollResultsResponse>(ct))!;
    }

    private HttpClient CreateAuthenticatedClient(string bearerToken)
    {
        var client = _httpClientFactory.CreateClient("InternalApi");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            bearerToken
        );
        return client;
    }

    private async Task EnsureSuccessAsync(
        HttpResponseMessage response,
        string operation,
        CancellationToken ct
    )
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        var body = await response.Content.ReadAsStringAsync(ct);
        _logger.LogWarning(
            "{Operation} failed ({Status}): {Body}",
            operation,
            (int)response.StatusCode,
            body
        );
        response.EnsureSuccessStatusCode();
    }
}
