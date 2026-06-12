namespace BSDCPolls.Contracts.Responses.Users;

/// <summary>Profile information for the currently authenticated user.</summary>
/// <param name="UserUid">The public GUID identifier for the user.</param>
/// <param name="Username">The system-generated username.</param>
/// <param name="CreatedOn">UTC timestamp when the account was created.</param>
public sealed record UserProfileResponse(Guid UserUid, string Username, DateTime CreatedOn);
