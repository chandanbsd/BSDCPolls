using BSDCPolls.Contracts.Requests.Auth;
using BSDCPolls.Contracts.Responses.Auth;

namespace BSDCPolls.Api.Business.Auth;

/// <summary>Domain service for account registration, login, and username management.</summary>
public interface IAuthService
{
    /// <summary>
    /// Registers a new account. Generates a synthetic internal email for GoTrue, auto-generates
    /// a profanity-free username, and persists the new <c>ApplicationUser</c>.
    /// </summary>
    Task<RegisterResponse> RegisterAsync(RegisterRequest request, CancellationToken ct = default);

    /// <summary>
    /// Authenticates by username and password. Resolves the stored synthetic email,
    /// calls GoTrue to obtain a JWT, and returns the access token with metadata.
    /// </summary>
    Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken ct = default);

    /// <summary>
    /// Generates a new username for an existing user, recording the old one in history.
    /// Rejects if the user has changed their username 3 or more times in the last 24 hours.
    /// <paramref name="supabaseUserId"/> is the synthetic email extracted from the JWT <c>email</c> claim.
    /// </summary>
    Task<string> ChangeUsernameAsync(string supabaseUserId, CancellationToken ct = default);
}
