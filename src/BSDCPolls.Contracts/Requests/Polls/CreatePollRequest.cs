using System.ComponentModel.DataAnnotations;

namespace BSDCPolls.Contracts.Requests.Polls;

/// <summary>Payload for creating a new poll.</summary>
/// <param name="Title">Poll title; 1–200 characters.</param>
/// <param name="IsPublic">Whether the poll appears in all users' feeds.</param>
public sealed record CreatePollRequest([Required] [MaxLength(200)] string Title, bool IsPublic);
