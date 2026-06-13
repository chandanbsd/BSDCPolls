using BSDCPolls.Contracts.Responses.Notifications;

namespace BSDCPolls.Api.Business.Invitations;

/// <summary>Domain service for creating poll and survey invitations.</summary>
public interface IInvitationService
{
    /// <summary>
    /// Creates an invitation to a poll for <paramref name="targetUsername"/> on behalf of <paramref name="inviterId"/>.
    /// Validates privacy settings and duplicate state before persisting.
    /// </summary>
    Task<InvitationResponse> CreatePollInvitationAsync(
        Guid pollUid,
        string targetUsername,
        int inviterId,
        CancellationToken ct = default);

    /// <summary>
    /// Creates an invitation to a survey for <paramref name="targetUsername"/> on behalf of <paramref name="inviterId"/>.
    /// Validates privacy settings and duplicate state before persisting.
    /// </summary>
    Task<InvitationResponse> CreateSurveyInvitationAsync(
        Guid surveyUid,
        string targetUsername,
        int inviterId,
        CancellationToken ct = default);
}
