using BSDCPolls.Api.Data.Entities;

namespace BSDCPolls.Api.Data.Repositories;

/// <summary>Data-access contract for <see cref="SurveyDocument"/> persistence operations.</summary>
public interface ISurveyDocumentRepository
{
    /// <summary>Persists a new <paramref name="document"/> and returns the tracked instance.</summary>
    /// <param name="document">The document to persist.</param>
    /// <param name="ct">Cancellation token.</param>
    Task<SurveyDocument> CreateAsync(SurveyDocument document, CancellationToken ct = default);

    /// <summary>
    /// Returns the document with the given <paramref name="uid"/> or <c>null</c> if not found.
    /// </summary>
    /// <param name="uid">Public GUID of the document.</param>
    /// <param name="ct">Cancellation token.</param>
    Task<SurveyDocument?> GetByUidAsync(Guid uid, CancellationToken ct = default);
}
