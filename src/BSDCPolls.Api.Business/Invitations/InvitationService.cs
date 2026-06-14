using System.Net.Http.Json;
using BSDCPolls.Api.Data;
using BSDCPolls.Api.Data.Entities;
using BSDCPolls.Api.Data.Repositories;
using BSDCPolls.Contracts.Enums;
using BSDCPolls.Contracts.Responses.Notifications;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BSDCPolls.Api.Business.Invitations;

/// <summary>Domain service for creating poll and survey invitations.</summary>
public sealed class InvitationService : IInvitationService
{
    private readonly IUserRepository _userRepository;
    private readonly IPollRepository _pollRepository;
    private readonly ISurveyRepository _surveyRepository;
    private readonly IInvitationRepository _invitationRepository;
    private readonly INotificationRepository _notificationRepository;
    private readonly BsdcPollsDbContext _db;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<InvitationService> _logger;

    /// <summary>Initialises the service with required repositories and infrastructure.</summary>
    public InvitationService(
        IUserRepository userRepository,
        IPollRepository pollRepository,
        ISurveyRepository surveyRepository,
        IInvitationRepository invitationRepository,
        INotificationRepository notificationRepository,
        BsdcPollsDbContext db,
        IHttpClientFactory httpClientFactory,
        ILogger<InvitationService> logger
    )
    {
        _userRepository = userRepository;
        _pollRepository = pollRepository;
        _surveyRepository = surveyRepository;
        _invitationRepository = invitationRepository;
        _notificationRepository = notificationRepository;
        _db = db;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<InvitationResponse> CreatePollInvitationAsync(
        Guid pollUid,
        string targetUsername,
        int inviterId,
        CancellationToken ct = default
    )
    {
        var poll =
            await _pollRepository.GetByUidAsync(pollUid, inviterId, ct)
            ?? throw new KeyNotFoundException($"Poll {pollUid} not found.");

        if (poll.CreatorId != inviterId)
        {
            throw new UnauthorizedAccessException("Only the poll creator may invite users.");
        }

        var result = await CreateInvitationCoreAsync(
            targetUsername,
            inviterId,
            pollId: poll.Id,
            surveyId: null,
            pollUid: poll.Uid,
            pollTitle: poll.Title,
            surveyUid: null,
            surveyTitle: null,
            ct
        );

        _logger.LogInformation(
            "User {InviterId} invited {InviteeUsername} to poll {PollUid}",
            inviterId,
            targetUsername,
            pollUid
        );

        return result;
    }

    /// <inheritdoc />
    public async Task<InvitationResponse> CreateSurveyInvitationAsync(
        Guid surveyUid,
        string targetUsername,
        int inviterId,
        CancellationToken ct = default
    )
    {
        var survey =
            await _surveyRepository.GetByUidAsync(surveyUid, inviterId, ct)
            ?? throw new KeyNotFoundException($"Survey {surveyUid} not found.");

        if (survey.CreatorId != inviterId)
        {
            throw new UnauthorizedAccessException("Only the survey creator may invite users.");
        }

        var result = await CreateInvitationCoreAsync(
            targetUsername,
            inviterId,
            pollId: null,
            surveyId: survey.Id,
            pollUid: null,
            pollTitle: null,
            surveyUid: survey.Uid,
            surveyTitle: survey.Title,
            ct
        );

        _logger.LogInformation(
            "User {InviterId} invited {InviteeUsername} to survey {SurveyUid}",
            inviterId,
            targetUsername,
            surveyUid
        );

        return result;
    }

    private async Task<InvitationResponse> CreateInvitationCoreAsync(
        string targetUsername,
        int inviterId,
        int? pollId,
        int? surveyId,
        Guid? pollUid,
        string? pollTitle,
        Guid? surveyUid,
        string? surveyTitle,
        CancellationToken ct
    )
    {
        var invitee =
            await _userRepository.GetByUsernameAsync(targetUsername, ct)
            ?? throw new KeyNotFoundException($"User '{targetUsername}' not found.");

        if (invitee.Id == inviterId)
        {
            throw new InvalidOperationException("Users may not invite themselves.");
        }

        await CheckPrivacyPermissionAsync(invitee.Id, inviterId, ct);

        var isDuplicate = await _invitationRepository.IsDuplicateAsync(
            invitee.Id,
            pollId,
            surveyId,
            ct
        );
        if (isDuplicate)
        {
            throw new InvalidOperationException(
                $"User '{targetUsername}' has already been invited."
            );
        }

        var inviter =
            await _db.ApplicationUsers.FirstOrDefaultAsync(u => u.Id == inviterId && u.IsActive, ct)
            ?? throw new InvalidOperationException("Inviter not found.");

        var invitation = pollId.HasValue
            ? Invitation.CreateForPoll(inviterId, invitee.Id, pollId.Value)
            : Invitation.CreateForSurvey(inviterId, invitee.Id, surveyId!.Value);

        await _invitationRepository.CreateAsync(invitation, ct);

        var notification = Notification.Create(invitee.Id, invitation.Id);
        await _notificationRepository.CreateAsync(notification, ct);

        await PushNotificationAsync(
            invitee.SupabaseUserId,
            new InvitationReceivedPayload(
                notification.Uid,
                inviter.Username,
                pollUid,
                pollTitle,
                surveyUid,
                surveyTitle,
                notification.CreatedOn
            ),
            ct
        );

        return new InvitationResponse(
            invitation.Uid,
            invitee.Username,
            invitee.Uid,
            invitation.CreatedOn
        );
    }

    private async Task CheckPrivacyPermissionAsync(
        int inviteeId,
        int inviterId,
        CancellationToken ct
    )
    {
        var settings = await _db.UserPrivacySettings.FirstOrDefaultAsync(
            p => p.UserId == inviteeId && p.IsActive,
            ct
        );

        if (settings is null)
        {
            return;
        }

        if (settings.InvitePermission == InvitePermission.Nobody)
        {
            throw new UnauthorizedAccessException("This user does not accept invitations.");
        }

        if (settings.InvitePermission == InvitePermission.AllowlistOnly)
        {
            var isAllowed = await _db.InviteAllowlistEntries.AnyAsync(
                e => e.OwnerId == inviteeId && e.AllowedUserId == inviterId && e.IsActive,
                ct
            );

            if (!isAllowed)
            {
                throw new UnauthorizedAccessException(
                    "This user only accepts invitations from approved users."
                );
            }
        }
    }

    private async Task PushNotificationAsync(
        string targetSupabaseId,
        InvitationReceivedPayload payload,
        CancellationToken ct
    )
    {
        try
        {
            var client = _httpClientFactory.CreateClient("BffInternal");
            await client.PostAsJsonAsync(
                "internal/notifications/push",
                new { targetSupabaseId, payload },
                ct
            );
        }
        catch (Exception ex)
        {
            _logger.LogWarning(
                ex,
                "Failed to push SignalR notification — notification is saved in the DB."
            );
        }
    }
}
