namespace BSDCPolls.Contracts.Responses.Auth;

/// <summary>
/// Returned after successful registration. Contains the auto-generated username.
/// No JWT is returned — the caller must invoke the login endpoint to obtain an access token.
/// </summary>
/// <param name="Username">The system-generated username, e.g. <c>swift-amber-moon</c>.</param>
/// <param name="UserUid">The public GUID identifier for the new user.</param>
public sealed record RegisterResponse(string Username, Guid UserUid);
