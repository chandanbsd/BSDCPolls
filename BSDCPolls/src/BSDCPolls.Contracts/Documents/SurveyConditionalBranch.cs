namespace BSDCPolls.Contracts.Documents;

/// <summary>
/// A conditional branch that activates when a specific parent choice is selected.
/// </summary>
/// <param name="ParentChoiceUid">The choice on the parent question that triggers this branch.</param>
/// <param name="Questions">Sub-questions shown only when this branch is triggered.</param>
public sealed record SurveyConditionalBranch(
    Guid ParentChoiceUid,
    IReadOnlyList<SurveyQuestionNode> Questions);
