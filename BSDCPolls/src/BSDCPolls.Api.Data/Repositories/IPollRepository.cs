using BSDCPolls.Api.Data.Entities;
using BSDCPolls.Contracts.Enums;

namespace BSDCPolls.Api.Data.Repositories;

/// <summary>Data-access contract for <see cref="Poll"/> persistence operations.</summary>
public interface IPollRepository
{
    /// <summary>
    /// Returns the poll with the given <paramref name="uid"/>, including eager-loaded
    /// questions and options, or <c>null</c> if not found.
    /// </summary>
    /// <param name="uid">Public GUID of the poll.</param>
    /// <param name="requestingUserId">Internal ID of the user requesting the poll (for access checks).</param>
    /// <param name="ct">Cancellation token.</param>
    Task<Poll?> GetByUidAsync(Guid uid, int? requestingUserId, CancellationToken ct = default);

    /// <summary>
    /// Returns a paginated list of polls visible to <paramref name="userId"/>.
    /// Includes public polls (when <paramref name="showPublic"/> is true) and
    /// polls the user has been invited to.
    /// </summary>
    Task<(IReadOnlyList<Poll> Items, int TotalCount)> GetFeedAsync(
        int userId,
        bool showPublic,
        PollStatus? status,
        int page,
        int pageSize,
        CancellationToken ct = default
    );

    /// <summary>
    /// Returns the poll with vote counts for all questions, restricted to the creator.
    /// </summary>
    Task<Poll?> GetWithSubmissionsAsync(
        Guid pollUid,
        int creatorId,
        CancellationToken ct = default
    );

    /// <summary>Persists a newly created <paramref name="poll"/> and returns the tracked instance.</summary>
    Task<Poll> CreateAsync(Poll poll, CancellationToken ct = default);

    /// <summary>Saves pending changes to <paramref name="poll"/> tracked by the current DbContext.</summary>
    Task UpdateAsync(Poll poll, CancellationToken ct = default);
}
