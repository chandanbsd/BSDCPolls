using BSDCPolls.Contracts.Requests.Auth;
using BSDCPolls.Contracts.Responses.Auth;
using BSDCPolls.Contracts.Responses.Users;

namespace BSDCPolls.BFF.Business.Auth;

/// <summary>BFF-layer auth service that proxies calls to the internal API.</summary>
public interface IBffAuthService
{
    /// <summary>Forwards a registration request to the internal API and returns the result.</summary>
    Task<RegisterResponse> RegisterAsync(RegisterRequest request, CancellationToken ct = default);

    /// <summary>Forwards a login request to the internal API and returns the JWT and user metadata.</summary>
    Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken ct = default);

    /// <summary>
    /// Fetches the authenticated user's profile from the internal API.
    /// <paramref name="bearerToken"/> is the GoTrue JWT sourced from the BFF request context.
    /// </summary>
    Task<UserProfileResponse> GetProfileAsync(string bearerToken, CancellationToken ct = default);

    /// <summary>
    /// Forwards a username-change request to the internal API on behalf of the authenticated user.
    /// <paramref name="bearerToken"/> is the GoTrue JWT sourced from the BFF request context.
    /// </summary>
    Task<UsernameChangeResponse> ChangeUsernameAsync(
        string bearerToken,
        CancellationToken ct = default
    );
}
