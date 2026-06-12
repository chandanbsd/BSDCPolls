namespace BSDCPolls.Contracts.Enums;

/// <summary>Status lifecycle of a survey.</summary>
public enum SurveyStatus
{
    /// <summary>Survey is being authored and not yet published.</summary>
    Draft = 0,

    /// <summary>Survey is published and accepting responses.</summary>
    Published = 1,

    /// <summary>Survey is closed; no further responses accepted.</summary>
    Closed = 2,
}
