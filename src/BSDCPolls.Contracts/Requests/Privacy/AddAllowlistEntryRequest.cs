using System.ComponentModel.DataAnnotations;

namespace BSDCPolls.Contracts.Requests.Privacy;

/// <summary>Request body for adding a user to the invite allowlist.</summary>
/// <param name="Username">The username of the user to allow.</param>
public sealed record AddAllowlistEntryRequest([Required] [MaxLength(60)] string Username);
