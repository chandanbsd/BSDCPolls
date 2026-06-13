using BSDCPolls.Api.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace BSDCPolls.Api.Data.Repositories;

/// <summary>EF Core implementation of <see cref="ISurveyResponseRepository"/>.</summary>
public sealed class SurveyResponseRepository : ISurveyResponseRepository
{
    private readonly BsdcPollsDbContext _db;

    /// <summary>Initialises the repository with the scoped <see cref="BsdcPollsDbContext"/>.</summary>
    public SurveyResponseRepository(BsdcPollsDbContext db)
    {
        _db = db;
    }

    /// <inheritdoc />
    public Task<SurveyResponse?> GetByRespondentAsync(
        Guid surveyUid,
        int respondentId,
        CancellationToken ct = default
    ) =>
        _db
            .SurveyResponses.Include(r => r.Documents)
            .FirstOrDefaultAsync(
                r => r.Survey.Uid == surveyUid && r.RespondentId == respondentId && r.IsActive,
                ct
            );

    /// <inheritdoc />
    public Task<bool> HasCompletedAsync(
        Guid surveyUid,
        int respondentId,
        CancellationToken ct = default
    ) =>
        _db.SurveyResponses.AnyAsync(
            r =>
                r.Survey.Uid == surveyUid
                && r.RespondentId == respondentId
                && r.IsComplete
                && r.IsActive,
            ct
        );

    /// <inheritdoc />
    public async Task<SurveyResponse> CreateAsync(
        SurveyResponse response,
        CancellationToken ct = default
    )
    {
        _db.SurveyResponses.Add(response);
        await _db.SaveChangesAsync(ct);
        return response;
    }

    /// <inheritdoc />
    public Task UpdateAsync(SurveyResponse response, CancellationToken ct = default) =>
        _db.SaveChangesAsync(ct).ContinueWith(_ => { }, ct);
}
