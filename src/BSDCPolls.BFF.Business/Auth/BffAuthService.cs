using System.Net.Http.Headers;
using System.Net.Http.Json;
using BSDCPolls.Contracts.Requests.Auth;
using BSDCPolls.Contracts.Responses.Auth;
using BSDCPolls.Contracts.Responses.Users;
using Microsoft.Extensions.Logging;

namespace BSDCPolls.BFF.Business.Auth;

/// <summary>
/// Forwards authentication requests to the internal API over the named <c>"InternalApi"</c> HttpClient.
/// The BFF never calls GoTrue directly — all auth logic lives in the internal API.
/// </summary>
public sealed class BffAuthService : IBffAuthService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<BffAuthService> _logger;

    /// <summary>Initialises the service with the HTTP client factory and logger.</summary>
    public BffAuthService(IHttpClientFactory httpClientFactory, ILogger<BffAuthService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<RegisterResponse> RegisterAsync(
        RegisterRequest request,
        CancellationToken ct = default
    )
    {
        var client = _httpClientFactory.CreateClient("InternalApi");
        var response = await client.PostAsJsonAsync("/api/internal/auth/register", request, ct);

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(ct);
            _logger.LogWarning(
                "Register forwarding failed ({Status}): {Body}",
                (int)response.StatusCode,
                body
            );
            throw new InvalidOperationException(
                $"Registration failed ({(int)response.StatusCode})."
            );
        }

        return await response.Content.ReadFromJsonAsync<RegisterResponse>(ct)
            ?? throw new InvalidOperationException(
                "Internal API returned empty register response."
            );
    }

    /// <inheritdoc />
    public async Task<LoginResponse> LoginAsync(
        LoginRequest request,
        CancellationToken ct = default
    )
    {
        var client = _httpClientFactory.CreateClient("InternalApi");
        var response = await client.PostAsJsonAsync("/api/internal/auth/login", request, ct);

        if (!response.IsSuccessStatusCode)
        {
            throw new UnauthorizedAccessException("Invalid username or password.");
        }

        return await response.Content.ReadFromJsonAsync<LoginResponse>(ct)
            ?? throw new InvalidOperationException("Internal API returned empty login response.");
    }

    /// <inheritdoc />
    public async Task<UserProfileResponse> GetProfileAsync(
        string bearerToken,
        CancellationToken ct = default
    )
    {
        var client = _httpClientFactory.CreateClient("InternalApi");
        using var requestMessage = new HttpRequestMessage(HttpMethod.Get, "/api/internal/users/me");
        requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);

        var response = await client.SendAsync(requestMessage, ct);

        if (!response.IsSuccessStatusCode)
        {
            throw new UnauthorizedAccessException("Unable to retrieve user profile.");
        }

        return await response.Content.ReadFromJsonAsync<UserProfileResponse>(ct)
            ?? throw new InvalidOperationException("Internal API returned empty profile response.");
    }

    /// <inheritdoc />
    public async Task<UsernameChangeResponse> ChangeUsernameAsync(
        string bearerToken,
        CancellationToken ct = default
    )
    {
        var client = _httpClientFactory.CreateClient("InternalApi");
        using var requestMessage = new HttpRequestMessage(
            HttpMethod.Post,
            "/api/internal/auth/change-username"
        );
        requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);

        var response = await client.SendAsync(requestMessage, ct);

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(ct);
            throw new InvalidOperationException(
                $"Username change failed ({(int)response.StatusCode}): {body}"
            );
        }

        return await response.Content.ReadFromJsonAsync<UsernameChangeResponse>(ct)
            ?? throw new InvalidOperationException(
                "Internal API returned empty username-change response."
            );
    }
}
