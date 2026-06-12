using System.Net.Http.Json;
using System.Text.Json.Serialization;
using BSDCPolls.Api.Business.UsernameGeneration;
using BSDCPolls.Api.Data.Entities;
using BSDCPolls.Api.Data.Repositories;
using BSDCPolls.Contracts.Requests.Auth;
using BSDCPolls.Contracts.Responses.Auth;
using Microsoft.Extensions.Logging;

namespace BSDCPolls.Api.Business.Auth;

/// <summary>
/// Implements account registration, login, and username management by coordinating
/// GoTrue (Supabase Auth), username generation, and the application user repository.
/// GoTrue is called over HTTP using the named <c>"GoTrue"</c> HttpClient.
/// </summary>
public sealed class AuthService : IAuthService
{
    private const int UsernameChangeRateLimitCount = 3;
    private const int UsernameChangeRateLimitWindowDays = 1;

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IUsernameGenerator _usernameGenerator;
    private readonly IUserRepository _userRepository;
    private readonly IUsernameHistoryRepository _usernameHistoryRepository;
    private readonly ILogger<AuthService> _logger;

    /// <summary>Initialises the service with all required dependencies.</summary>
    public AuthService(
        IHttpClientFactory httpClientFactory,
        IUsernameGenerator usernameGenerator,
        IUserRepository userRepository,
        IUsernameHistoryRepository usernameHistoryRepository,
        ILogger<AuthService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _usernameGenerator = usernameGenerator;
        _userRepository = userRepository;
        _usernameHistoryRepository = usernameHistoryRepository;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<RegisterResponse> RegisterAsync(RegisterRequest request, CancellationToken ct = default)
    {
        // Synthetic email is never shown to users — it is the internal GoTrue credential key.
        var syntheticEmail = $"{Guid.NewGuid():N}@internal.bsdcpolls";

        var goTrueUser = await SignUpWithGoTrueAsync(syntheticEmail, request.Password, ct);

        var username = await _usernameGenerator.GenerateAsync(
            candidate => _userRepository.UsernameExistsAsync(candidate, ct),
            ct);

        var user = ApplicationUser.Create(username, syntheticEmail);
        await _userRepository.CreateAsync(user, ct);

        _logger.LogInformation(
            "New user registered. UserUid={UserUid} GoTrueId={GoTrueId}",
            user.Uid,
            goTrueUser.Id);

        return new RegisterResponse(user.Username, user.Uid);
    }

    /// <inheritdoc />
    public async Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken ct = default)
    {
        var user = await _userRepository.GetByUsernameAsync(request.Username, ct)
            ?? throw new UnauthorizedAccessException("Invalid username or password.");

        var token = await GetGoTrueTokenAsync(user.SupabaseUserId, request.Password, ct);

        _logger.LogInformation("User logged in. UserUid={UserUid}", user.Uid);

        var expiresAt = DateTime.UtcNow.AddSeconds(token.ExpiresIn);
        return new LoginResponse(token.AccessToken, expiresAt, user.Uid, user.Username);
    }

    /// <inheritdoc />
    public async Task<string> ChangeUsernameAsync(string supabaseUserId, CancellationToken ct = default)
    {
        var user = await _userRepository.GetBySupabaseIdAsync(supabaseUserId, ct)
            ?? throw new KeyNotFoundException($"User with supabase ID '{supabaseUserId}' not found.");

        var recentChanges = await _usernameHistoryRepository
            .CountRecentChangesAsync(user.Id, UsernameChangeRateLimitWindowDays, ct);

        if (recentChanges >= UsernameChangeRateLimitCount)
        {
            throw new InvalidOperationException(
                "Username change limit reached. You may change your username at most 3 times per 24 hours.");
        }

        var oldUsername = user.Username;

        var newUsername = await _usernameGenerator.GenerateAsync(
            candidate => _userRepository.UsernameExistsAsync(candidate, ct),
            ct);

        var history = UsernameHistory.Create(user.Id, oldUsername, DateTime.UtcNow);
        await _usernameHistoryRepository.AddAsync(history, ct);

        user.UpdateUsername(newUsername);
        await _userRepository.UpdateAsync(user, ct);

        _logger.LogInformation(
            "Username changed. UserUid={UserUid} OldUsername={OldUsername} NewUsername={NewUsername}",
            user.Uid,
            oldUsername,
            newUsername);

        return newUsername;
    }

    private async Task<GoTrueSignUpResponse> SignUpWithGoTrueAsync(
        string email,
        string password,
        CancellationToken ct)
    {
        var client = _httpClientFactory.CreateClient("GoTrue");
        var response = await client.PostAsJsonAsync("/signup", new GoTrueCredential(email, password), ct);

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(ct);
            throw new InvalidOperationException(
                $"GoTrue signup failed ({(int)response.StatusCode}): {body}");
        }

        return await response.Content.ReadFromJsonAsync<GoTrueSignUpResponse>(ct)
            ?? throw new InvalidOperationException("GoTrue signup returned an empty response.");
    }

    private async Task<GoTrueTokenResponse> GetGoTrueTokenAsync(
        string email,
        string password,
        CancellationToken ct)
    {
        var client = _httpClientFactory.CreateClient("GoTrue");
        var response = await client.PostAsJsonAsync(
            "/token?grant_type=password",
            new GoTrueCredential(email, password),
            ct);

        if (!response.IsSuccessStatusCode)
            throw new UnauthorizedAccessException("Invalid username or password.");

        return await response.Content.ReadFromJsonAsync<GoTrueTokenResponse>(ct)
            ?? throw new InvalidOperationException("GoTrue token endpoint returned an empty response.");
    }

    // ── GoTrue internal DTOs ──────────────────────────────────────────────────

    private sealed record GoTrueCredential(
        [property: JsonPropertyName("email")] string Email,
        [property: JsonPropertyName("password")] string Password);

    private sealed record GoTrueSignUpResponse(
        [property: JsonPropertyName("id")] string Id);

    private sealed record GoTrueTokenResponse(
        [property: JsonPropertyName("access_token")] string AccessToken,
        [property: JsonPropertyName("expires_in")] int ExpiresIn);
}
