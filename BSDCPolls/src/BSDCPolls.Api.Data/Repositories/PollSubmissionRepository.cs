using BSDCPolls.Api.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace BSDCPolls.Api.Data.Repositories;

/// <summary>EF Core implementation of <see cref="IPollSubmissionRepository"/>.</summary>
public sealed class PollSubmissionRepository : IPollSubmissionRepository
{
    private readonly BsdcPollsDbContext _db;

    /// <summary>Initialises the repository with the scoped <see cref="BsdcPollsDbContext"/>.</summary>
    public PollSubmissionRepository(BsdcPollsDbContext db)
    {
        _db = db;
    }

    /// <inheritdoc />
    public Task<bool> HasUserSubmittedAsync(int questionId, int userId, CancellationToken ct = default) =>
        _db.PollSubmissions
            .AnyAsync(s => s.PollQuestionId == questionId && s.RespondentId == userId && s.IsActive, ct);

    /// <inheritdoc />
    public async Task<IReadOnlyDictionary<Guid, int>> GetVoteCountsAsync(int questionId, CancellationToken ct = default)
    {
        var counts = await _db.PollSubmissions
            .Where(s => s.PollQuestionId == questionId && s.IsActive)
            .GroupBy(s => s.SelectedOption.Uid)
            .Select(g => new { OptionUid = g.Key, Count = g.Count() })
            .ToListAsync(ct);

        return counts.ToDictionary(x => x.OptionUid, x => x.Count);
    }

    /// <inheritdoc />
    public async Task<PollSubmission> CreateAsync(PollSubmission submission, CancellationToken ct = default)
    {
        _db.PollSubmissions.Add(submission);
        await _db.SaveChangesAsync(ct);
        return submission;
    }
}
