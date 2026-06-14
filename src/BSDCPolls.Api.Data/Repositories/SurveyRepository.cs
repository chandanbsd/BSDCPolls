using BSDCPolls.Api.Data.Entities;
using BSDCPolls.Contracts.Enums;
using Microsoft.EntityFrameworkCore;

namespace BSDCPolls.Api.Data.Repositories;

/// <summary>EF Core implementation of <see cref="ISurveyRepository"/>.</summary>
public sealed class SurveyRepository : ISurveyRepository
{
    private readonly BsdcPollsDbContext _db;

    /// <summary>Initialises the repository with the scoped <see cref="BsdcPollsDbContext"/>.</summary>
    public SurveyRepository(BsdcPollsDbContext db)
    {
        _db = db;
    }

    /// <inheritdoc />
    public Task<Survey?> GetByUidAsync(
        Guid uid,
        int? requestingUserId,
        CancellationToken ct = default
    ) =>
        _db
            .Surveys.Include(s => s.Creator)
            .FirstOrDefaultAsync(s => s.Uid == uid && s.IsActive, ct);

    /// <inheritdoc />
    public async Task<(IReadOnlyList<Survey> Items, int TotalCount)> GetFeedAsync(
        int userId,
        bool showPublic,
        SurveyStatus? status,
        int page,
        int pageSize,
        CancellationToken ct = default
    )
    {
        var query = _db
            .Surveys.Include(s => s.Creator)
            .Where(s =>
                s.IsActive
                && (
                    s.CreatorId == userId
                    || (showPublic && s.IsPublic)
                    || _db.Invitations.Any(i =>
                        i.SurveyId == s.Id && i.InviteeId == userId && i.IsActive
                    )
                )
            );

        if (status.HasValue)
        {
            query = query.Where(s => s.Status == status.Value);
        }

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(s => s.CreatedOn)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (items, totalCount);
    }

    /// <inheritdoc />
    public Task<Survey?> GetResultsAsync(
        Guid surveyUid,
        int creatorId,
        CancellationToken ct = default
    ) =>
        _db
            .Surveys.Include(s => s.Responses)
            .ThenInclude(r => r.Documents)
            .FirstOrDefaultAsync(
                s => s.Uid == surveyUid && s.CreatorId == creatorId && s.IsActive,
                ct
            );

    /// <inheritdoc />
    public async Task<Survey> CreateAsync(Survey survey, CancellationToken ct = default)
    {
        _db.Surveys.Add(survey);
        await _db.SaveChangesAsync(ct);
        return survey;
    }

    /// <inheritdoc />
    public Task UpdateAsync(Survey survey, CancellationToken ct = default) =>
        _db.SaveChangesAsync(ct).ContinueWith(_ => { }, ct);
}
