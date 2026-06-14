namespace BSDCPolls.Contracts.Responses.Polls;

/// <summary>A single answer option within a poll question response.</summary>
/// <param name="OptionUid">Public GUID identifier for the option.</param>
/// <param name="Text">Option display text.</param>
/// <param name="OrderIndex">Display order.</param>
public sealed record PollAnswerOptionResponse(Guid OptionUid, string Text, int OrderIndex);
