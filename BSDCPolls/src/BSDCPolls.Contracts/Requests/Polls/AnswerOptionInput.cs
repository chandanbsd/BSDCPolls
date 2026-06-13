using System.ComponentModel.DataAnnotations;

namespace BSDCPolls.Contracts.Requests.Polls;

/// <summary>A single answer option when adding a poll question.</summary>
/// <param name="Text">Option text; 1–200 characters.</param>
/// <param name="OrderIndex">Display order; must be ≥ 0.</param>
public sealed record AnswerOptionInput(
    [Required] [MaxLength(200)] string Text,
    int OrderIndex);
