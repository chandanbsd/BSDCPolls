namespace BSDCPolls.Contracts.Responses.Surveys;

/// <summary>Vote count for a single multiple-choice option.</summary>
/// <param name="ChoiceUid">Public GUID of the choice option.</param>
/// <param name="Text">Display text of the choice.</param>
/// <param name="Count">Number of respondents who selected this choice.</param>
public sealed record SurveyChoiceTally(Guid ChoiceUid, string Text, int Count);
