using System.ComponentModel.DataAnnotations;

namespace BSDCPolls.Contracts.Requests.Auth;

/// <summary>
/// Payload for authenticating with a username and password.
/// </summary>
/// <param name="Username">The system-generated username assigned at registration.</param>
/// <param name="Password">The user's password.</param>
public sealed record LoginRequest(
    [Required] [MaxLength(60)] string Username,
    [Required] string Password);
