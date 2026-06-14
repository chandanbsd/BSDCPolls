using BSDCPolls.Api.Data.Entities;

namespace BSDCPolls.Api.Data.Repositories;

/// <summary>Data-access contract for <see cref="PollSubmission"/> persistence operations.</summary>
public interface IPollSubmissionRepository
{
    /// <summary>Returns <c>true</c> if <paramref name="userId"/> has already answered <paramref name="questionId"/>.</summary>
    Task<bool> HasUserSubmittedAsync(int questionId, int userId, CancellationToken ct = default);

    /// <summary>
    /// Returns per-option vote counts for <paramref name="questionId"/>
    /// as a dictionary keyed by option GUID.
    /// </summary>
    Task<IReadOnlyDictionary<Guid, int>> GetVoteCountsAsync(
        int questionId,
        CancellationToken ct = default
    );

    /// <summary>Persists a new submission and returns the tracked instance.</summary>
    Task<PollSubmission> CreateAsync(PollSubmission submission, CancellationToken ct = default);
}
