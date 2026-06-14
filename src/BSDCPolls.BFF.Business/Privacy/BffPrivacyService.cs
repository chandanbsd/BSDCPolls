using System.Net.Http.Headers;
using System.Net.Http.Json;
using BSDCPolls.Contracts.Requests.Privacy;
using BSDCPolls.Contracts.Responses.Users;
using Microsoft.Extensions.Logging;

namespace BSDCPolls.BFF.Business.Privacy;

/// <summary>Forwards privacy and user profile requests from the BFF to the internal API.</summary>
public sealed class BffPrivacyService : IBffPrivacyService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<BffPrivacyService> _logger;

    /// <summary>Initialises the service with the HTTP client factory and logger.</summary>
    public BffPrivacyService(
        IHttpClientFactory httpClientFactory,
        ILogger<BffPrivacyService> logger
    )
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<PrivacySettingsResponse> GetPrivacySettingsAsync(
        string bearerToken,
        CancellationToken ct = default
    )
    {
        var client = CreateAuthenticatedClient(bearerToken);
        var response = await client.GetAsync("/api/internal/users/me/privacy", ct);
        await EnsureSuccessAsync(response, "Get privacy settings", ct);
        return (await response.Content.ReadFromJsonAsync<PrivacySettingsResponse>(ct))!;
    }

    /// <inheritdoc />
    public async Task<PrivacySettingsResponse> UpdatePrivacySettingsAsync(
        UpdatePrivacySettingsRequest request,
        string bearerToken,
        CancellationToken ct = default
    )
    {
        var client = CreateAuthenticatedClient(bearerToken);
        var response = await client.PutAsJsonAsync("/api/internal/users/me/privacy", request, ct);
        await EnsureSuccessAsync(response, "Update privacy settings", ct);
        return (await response.Content.ReadFromJsonAsync<PrivacySettingsResponse>(ct))!;
    }

    /// <inheritdoc />
    public async Task<AllowlistEntryResponse> AddAllowlistEntryAsync(
        AddAllowlistEntryRequest request,
        string bearerToken,
        CancellationToken ct = default
    )
    {
        var client = CreateAuthenticatedClient(bearerToken);
        var response = await client.PostAsJsonAsync(
            "/api/internal/users/me/privacy/allowlist",
            request,
            ct
        );
        await EnsureSuccessAsync(response, "Add allowlist entry", ct);
        return (await response.Content.ReadFromJsonAsync<AllowlistEntryResponse>(ct))!;
    }

    /// <inheritdoc />
    public async Task RemoveAllowlistEntryAsync(
        Guid allowedUserUid,
        string bearerToken,
        CancellationToken ct = default
    )
    {
        var client = CreateAuthenticatedClient(bearerToken);
        var response = await client.DeleteAsync(
            $"/api/internal/users/me/privacy/allowlist/{allowedUserUid}",
            ct
        );
        await EnsureSuccessAsync(response, "Remove allowlist entry", ct);
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
