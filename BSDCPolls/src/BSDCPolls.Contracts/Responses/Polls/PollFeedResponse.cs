namespace BSDCPolls.Contracts.Responses.Polls;

/// <summary>Paginated list of polls visible to the current user.</summary>
/// <param name="Items">Poll summaries for the current page.</param>
/// <param name="TotalCount">Total matching polls across all pages.</param>
/// <param name="Page">Current page number (1-based).</param>
/// <param name="PageSize">Number of items per page.</param>
public sealed record PollFeedResponse(
    IReadOnlyList<PollFeedItem> Items,
    int TotalCount,
    int Page,
    int PageSize);
