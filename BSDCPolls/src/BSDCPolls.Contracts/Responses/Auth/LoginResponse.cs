namespace BSDCPolls.Contracts.Responses.Auth;

/// <summary>
/// Returned after successful authentication.
/// </summary>
/// <param name="AccessToken">Supabase GoTrue JWT for use in subsequent authenticated requests.</param>
/// <param name="ExpiresAt">UTC timestamp when the access token expires.</param>
/// <param name="UserUid">The public GUID identifier for the authenticated user.</param>
/// <param name="Username">The system-generated username of the authenticated user.</param>
public sealed record LoginResponse(string AccessToken, DateTime ExpiresAt, Guid UserUid, string Username);
