using BSDCPolls.Contracts.Requests.Invitations;
using BSDCPolls.Contracts.Responses.Notifications;

namespace BSDCPolls.BFF.Business.Invitations;

/// <summary>BFF service that forwards invitation requests to the internal API.</summary>
public interface IBffInvitationService
{
    /// <summary>Invites a user to the specified poll on behalf of the current user.</summary>
    Task<InvitationResponse> CreatePollInvitationAsync(Guid pollUid, CreateInvitationRequest request, string bearerToken, CancellationToken ct = default);

    /// <summary>Invites a user to the specified survey on behalf of the current user.</summary>
    Task<InvitationResponse> CreateSurveyInvitationAsync(Guid surveyUid, CreateInvitationRequest request, string bearerToken, CancellationToken ct = default);
}
