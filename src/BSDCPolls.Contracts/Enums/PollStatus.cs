namespace BSDCPolls.Contracts.Enums;

/// <summary>Status lifecycle of a live poll session.</summary>
public enum PollStatus
{
    /// <summary>Poll is created but not yet live.</summary>
    Draft = 0,

    /// <summary>Poll is live and accepting questions and votes.</summary>
    Active = 1,

    /// <summary>Poll is closed; no further questions or votes accepted.</summary>
    Closed = 2,
}
