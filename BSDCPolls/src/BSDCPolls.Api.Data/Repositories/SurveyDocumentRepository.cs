using BSDCPolls.Api.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace BSDCPolls.Api.Data.Repositories;

/// <summary>EF Core implementation of <see cref="ISurveyDocumentRepository"/>.</summary>
public sealed class SurveyDocumentRepository : ISurveyDocumentRepository
{
    private readonly BsdcPollsDbContext _db;

    /// <summary>Initialises the repository with the scoped <see cref="BsdcPollsDbContext"/>.</summary>
    public SurveyDocumentRepository(BsdcPollsDbContext db)
    {
        _db = db;
    }

    /// <inheritdoc />
    public async Task<SurveyDocument> CreateAsync(SurveyDocument document, CancellationToken ct = default)
    {
        _db.SurveyDocuments.Add(document);
        await _db.SaveChangesAsync(ct);
        return document;
    }

    /// <inheritdoc />
    public Task<SurveyDocument?> GetByUidAsync(Guid uid, CancellationToken ct = default) =>
        _db.SurveyDocuments.FirstOrDefaultAsync(d => d.Uid == uid && d.IsActive, ct);
}
