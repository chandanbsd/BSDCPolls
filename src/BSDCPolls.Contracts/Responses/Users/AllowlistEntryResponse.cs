namespace BSDCPolls.Contracts.Responses.Users;

/// <summary>A single entry in the user's invite allowlist.</summary>
/// <param name="UserUid">Public GUID of the allowed user.</param>
/// <param name="Username">Username of the allowed user.</param>
public sealed record AllowlistEntryResponse(Guid UserUid, string Username);
