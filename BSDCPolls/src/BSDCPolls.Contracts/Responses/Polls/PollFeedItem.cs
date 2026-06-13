using BSDCPolls.Contracts.Enums;

namespace BSDCPolls.Contracts.Responses.Polls;

/// <summary>A poll summary shown in the feed.</summary>
/// <param name="PollUid">Public GUID identifier.</param>
/// <param name="Title">Poll title.</param>
/// <param name="IsPublic">Whether the poll is public.</param>
/// <param name="Status">Current poll status.</param>
/// <param name="CreatorUsername">Username of the poll creator.</param>
/// <param name="QuestionCount">Total number of questions added.</param>
/// <param name="CreatedOn">UTC creation timestamp.</param>
/// <param name="InvitedAt">UTC time the current user was invited; null for public access.</param>
public sealed record PollFeedItem(
    Guid PollUid,
    string Title,
    bool IsPublic,
    PollStatus Status,
    string CreatorUsername,
    int QuestionCount,
    DateTime CreatedOn,
    DateTime? InvitedAt
);
