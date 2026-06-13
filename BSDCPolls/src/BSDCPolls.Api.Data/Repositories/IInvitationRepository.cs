using BSDCPolls.Api.Data.Entities;

namespace BSDCPolls.Api.Data.Repositories;

/// <summary>Data-access contract for <see cref="Invitation"/> persistence operations.</summary>
public interface IInvitationRepository
{
    /// <summary>Persists a new <paramref name="invitation"/> and returns the tracked instance.</summary>
    Task<Invitation> CreateAsync(Invitation invitation, CancellationToken ct = default);

    /// <summary>Returns the invitation with the given public <paramref name="uid"/>, or <c>null</c> if not found.</summary>
    Task<Invitation?> GetByUidAsync(Guid uid, CancellationToken ct = default);

    /// <summary>
    /// Returns <c>true</c> if an active invitation already exists for the given invitee
    /// and the specified poll or survey.
    /// </summary>
    Task<bool> IsDuplicateAsync(int inviteeId, int? pollId, int? surveyId, CancellationToken ct = default);

    /// <summary>Returns the invitation for a specific poll and invitee, or <c>null</c>.</summary>
    Task<Invitation?> GetForPollAsync(Guid pollUid, int inviteeId, CancellationToken ct = default);

    /// <summary>Returns the invitation for a specific survey and invitee, or <c>null</c>.</summary>
    Task<Invitation?> GetForSurveyAsync(Guid surveyUid, int inviteeId, CancellationToken ct = default);
}
