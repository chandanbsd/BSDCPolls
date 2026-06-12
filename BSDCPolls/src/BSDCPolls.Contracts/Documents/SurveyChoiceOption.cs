namespace BSDCPolls.Contracts.Documents;

/// <summary>
/// A single selectable option for a multiple-choice question.
/// </summary>
/// <param name="Uid">Stable identifier for this choice option.</param>
/// <param name="Text">The display text for this choice.</param>
public sealed record SurveyChoiceOption(Guid Uid, string Text);
