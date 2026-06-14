namespace BSDCPolls.Contracts.Requests.Polls;

/// <summary>Payload for submitting a vote on a poll question.</summary>
/// <param name="QuestionUid">The GUID of the question being answered.</param>
/// <param name="SelectedOptionUid">The GUID of the chosen answer option.</param>
public sealed record SubmitPollVoteRequest(Guid QuestionUid, Guid SelectedOptionUid);
