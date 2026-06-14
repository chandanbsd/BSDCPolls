namespace BSDCPolls.Contracts.Responses.Notifications;

/// <summary>Invitation details embedded in a notification item.</summary>
/// <param name="InvitationUid">Public GUID of the invitation.</param>
/// <param name="InviterUsername">Username of the user who sent the invitation.</param>
/// <param name="PollUid">Public GUID of the invited poll; null for survey invitations.</param>
/// <param name="PollTitle">Title of the invited poll; null for survey invitations.</param>
/// <param name="SurveyUid">Public GUID of the invited survey; null for poll invitations.</param>
/// <param name="SurveyTitle">Title of the invited survey; null for poll invitations.</param>
public sealed record NotificationInvitationDetail(
    Guid InvitationUid,
    string InviterUsername,
    Guid? PollUid,
    string? PollTitle,
    Guid? SurveyUid,
    string? SurveyTitle
);
