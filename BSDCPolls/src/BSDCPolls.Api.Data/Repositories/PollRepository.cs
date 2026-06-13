using BSDCPolls.Api.Data.Entities;
using BSDCPolls.Contracts.Enums;
using Microsoft.EntityFrameworkCore;

namespace BSDCPolls.Api.Data.Repositories;

/// <summary>EF Core implementation of <see cref="IPollRepository"/>.</summary>
public sealed class PollRepository : IPollRepository
{
    private readonly BsdcPollsDbContext _db;

    /// <summary>Initialises the repository with the scoped <see cref="BsdcPollsDbContext"/>.</summary>
    public PollRepository(BsdcPollsDbContext db)
    {
        _db = db;
    }

    /// <inheritdoc />
    public Task<Poll?> GetByUidAsync(
        Guid uid,
        int? requestingUserId,
        CancellationToken ct = default
    ) =>
        _db
            .Polls.Include(p => p.Questions)
            .ThenInclude(q => q.AnswerOptions)
            .FirstOrDefaultAsync(p => p.Uid == uid && p.IsActive, ct);

    /// <inheritdoc />
    public async Task<(IReadOnlyList<Poll> Items, int TotalCount)> GetFeedAsync(
        int userId,
        bool showPublic,
        PollStatus? status,
        int page,
        int pageSize,
        CancellationToken ct = default
    )
    {
        var query = _db
            .Polls.Include(p => p.Creator)
            .Include(p => p.Questions)
            .Where(p =>
                p.IsActive
                && (
                    p.CreatorId == userId
                    || (showPublic && p.IsPublic)
                    || _db.Invitations.Any(i =>
                        i.PollId == p.Id && i.InviteeId == userId && i.IsActive
                    )
                )
            );

        if (status.HasValue)
        {
            query = query.Where(p => p.Status == status.Value);
        }

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(p => p.CreatedOn)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (items, totalCount);
    }

    /// <inheritdoc />
    public Task<Poll?> GetWithSubmissionsAsync(
        Guid pollUid,
        int creatorId,
        CancellationToken ct = default
    ) =>
        _db
            .Polls.Include(p => p.Questions)
            .ThenInclude(q => q.AnswerOptions)
            .Include(p => p.Questions)
            .ThenInclude(q => q.Submissions)
            .FirstOrDefaultAsync(
                p => p.Uid == pollUid && p.CreatorId == creatorId && p.IsActive,
                ct
            );

    /// <inheritdoc />
    public async Task<Poll> CreateAsync(Poll poll, CancellationToken ct = default)
    {
        _db.Polls.Add(poll);
        await _db.SaveChangesAsync(ct);
        return poll;
    }

    /// <inheritdoc />
    public Task UpdateAsync(Poll poll, CancellationToken ct = default) =>
        _db.SaveChangesAsync(ct).ContinueWith(_ => { }, ct);
}
