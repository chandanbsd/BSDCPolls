using System.Net.Http.Headers;
using System.Net.Http.Json;
using BSDCPolls.Contracts.Requests.Invitations;
using BSDCPolls.Contracts.Responses.Notifications;
using Microsoft.Extensions.Logging;

namespace BSDCPolls.BFF.Business.Invitations;

/// <summary>Forwards invitation requests from the BFF to the internal API.</summary>
public sealed class BffInvitationService : IBffInvitationService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<BffInvitationService> _logger;

    /// <summary>Initialises the service with the HTTP client factory and logger.</summary>
    public BffInvitationService(IHttpClientFactory httpClientFactory, ILogger<BffInvitationService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<InvitationResponse> CreatePollInvitationAsync(
        Guid pollUid,
        CreateInvitationRequest request,
        string bearerToken,
        CancellationToken ct = default)
    {
        var client = CreateAuthenticatedClient(bearerToken);
        var response = await client.PostAsJsonAsync($"/api/internal/polls/{pollUid}/invitations", request, ct);
        await EnsureSuccessAsync(response, "Create poll invitation", ct);
        return (await response.Content.ReadFromJsonAsync<InvitationResponse>(ct))!;
    }

    /// <inheritdoc />
    public async Task<InvitationResponse> CreateSurveyInvitationAsync(
        Guid surveyUid,
        CreateInvitationRequest request,
        string bearerToken,
        CancellationToken ct = default)
    {
        var client = CreateAuthenticatedClient(bearerToken);
        var response = await client.PostAsJsonAsync($"/api/internal/surveys/{surveyUid}/invitations", request, ct);
        await EnsureSuccessAsync(response, "Create survey invitation", ct);
        return (await response.Content.ReadFromJsonAsync<InvitationResponse>(ct))!;
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
