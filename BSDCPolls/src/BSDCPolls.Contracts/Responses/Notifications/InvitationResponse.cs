namespace BSDCPolls.Contracts.Responses.Notifications;

/// <summary>Confirmation returned after successfully creating an invitation.</summary>
/// <param name="InvitationUid">Public GUID of the new invitation record.</param>
/// <param name="TargetUsername">The username that was invited.</param>
/// <param name="TargetUserUid">Public GUID of the invited user.</param>
/// <param name="CreatedOn">UTC creation timestamp.</param>
public sealed record InvitationResponse(
    Guid InvitationUid,
    string TargetUsername,
    Guid TargetUserUid,
    DateTime CreatedOn
);
