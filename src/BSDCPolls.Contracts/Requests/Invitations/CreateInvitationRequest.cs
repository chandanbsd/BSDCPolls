using System.ComponentModel.DataAnnotations;

namespace BSDCPolls.Contracts.Requests.Invitations;

/// <summary>Payload for inviting a user to a poll or survey by their username.</summary>
/// <param name="TargetUsername">The username of the user to invite; 1–60 characters.</param>
public sealed record CreateInvitationRequest([Required] [MaxLength(60)] string TargetUsername);
