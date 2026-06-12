namespace BSDCPolls.Contracts.Responses.Users;

/// <summary>Returned after a successful username change request.</summary>
/// <param name="NewUsername">The newly assigned system-generated username.</param>
public sealed record UsernameChangeResponse(string NewUsername);
