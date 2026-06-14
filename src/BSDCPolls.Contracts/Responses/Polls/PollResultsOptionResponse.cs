namespace BSDCPolls.Contracts.Responses.Polls;

/// <summary>Vote count breakdown for a single answer option.</summary>
/// <param name="OptionUid">Public GUID of the option.</param>
/// <param name="Text">Option display text.</param>
/// <param name="VoteCount">Number of votes received.</param>
/// <param name="Percentage">Vote share as a percentage (0.00–100.00).</param>
public sealed record PollResultsOptionResponse(
    Guid OptionUid,
    string Text,
    int VoteCount,
    decimal Percentage
);
