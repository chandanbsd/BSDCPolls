namespace BSDCPolls.Contracts.Responses.Surveys;

/// <summary>Paginated list of survey feed items visible to the current user.</summary>
/// <param name="Items">Page of survey summaries.</param>
/// <param name="TotalCount">Total matching surveys across all pages.</param>
/// <param name="Page">Current page number (1-based).</param>
/// <param name="PageSize">Number of items per page.</param>
public sealed record SurveyFeedResponse(
    IReadOnlyList<SurveyFeedItem> Items,
    int TotalCount,
    int Page,
    int PageSize);
