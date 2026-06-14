using BSDCPolls.Contracts.Enums;

namespace BSDCPolls.Contracts.Requests.Surveys;

/// <summary>Payload for transitioning a survey's lifecycle status.</summary>
/// <param name="Status">The target status (Published or Closed).</param>
public sealed record ChangeSurveyStatusRequest(SurveyStatus Status);
