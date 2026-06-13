namespace BSDCPolls.Contracts.Responses.Notifications;

/// <summary>SignalR push payload sent to a user when they receive a new invitation.</summary>
/// <param name="NotificationUid">Public GUID of the newly created notification.</param>
/// <param name="InviterUsername">Username of the user who sent the invitation.</param>
/// <param name="PollUid">Public GUID of the invited poll; null for survey invitations.</param>
/// <param name="PollTitle">Title of the invited poll; null for survey invitations.</param>
/// <param name="SurveyUid">Public GUID of the invited survey; null for poll invitations.</param>
/// <param name="SurveyTitle">Title of the invited survey; null for poll invitations.</param>
/// <param name="CreatedOn">UTC timestamp the notification was created.</param>
public sealed record InvitationReceivedPayload(
    Guid NotificationUid,
    string InviterUsername,
    Guid? PollUid,
    string? PollTitle,
    Guid? SurveyUid,
    string? SurveyTitle,
    DateTime CreatedOn);
