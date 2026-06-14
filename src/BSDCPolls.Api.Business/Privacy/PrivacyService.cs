using BSDCPolls.Api.Data.Entities;
using BSDCPolls.Api.Data.Repositories;
using BSDCPolls.Contracts.Requests.Privacy;
using BSDCPolls.Contracts.Responses.Users;
using Microsoft.Extensions.Logging;

namespace BSDCPolls.Api.Business.Privacy;

/// <summary>Domain service for managing user privacy settings and invite allowlists.</summary>
public sealed class PrivacyService : IPrivacyService
{
    private readonly IPrivacySettingsRepository _privacyRepository;
    private readonly IUserRepository _userRepository;
    private readonly ILogger<PrivacyService> _logger;

    /// <summary>Initialises the service with required repositories and logger.</summary>
    public PrivacyService(
        IPrivacySettingsRepository privacyRepository,
        IUserRepository userRepository,
        ILogger<PrivacyService> logger
    )
    {
        _privacyRepository = privacyRepository;
        _userRepository = userRepository;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<PrivacySettingsResponse> GetSettingsAsync(
        int userId,
        CancellationToken ct = default
    )
    {
        var settings = await _privacyRepository.GetByUserIdAsync(userId, ct);

        if (settings is null)
        {
            settings = UserPrivacySettings.CreateDefault(userId);
            await _privacyRepository.CreateAsync(settings, ct);
        }

        var allowlist = await _privacyRepository.GetAllowlistAsync(userId, ct);

        return MapToResponse(settings, allowlist);
    }

    /// <inheritdoc />
    public async Task<PrivacySettingsResponse> UpdateSettingsAsync(
        int userId,
        UpdatePrivacySettingsRequest request,
        CancellationToken ct = default
    )
    {
        var settings = await _privacyRepository.GetByUserIdAsync(userId, ct);

        if (settings is null)
        {
            settings = UserPrivacySettings.CreateDefault(userId);
            await _privacyRepository.CreateAsync(settings, ct);
        }

        settings.Update(request.ShowPublicContent, request.InvitePermission);
        await _privacyRepository.UpdateAsync(settings, ct);

        _logger.LogInformation("User {UserId} updated privacy settings", userId);

        var allowlist = await _privacyRepository.GetAllowlistAsync(userId, ct);
        return MapToResponse(settings, allowlist);
    }

    /// <inheritdoc />
    public async Task<AllowlistEntryResponse> AddAllowlistEntryAsync(
        int ownerId,
        string targetUsername,
        CancellationToken ct = default
    )
    {
        var targetUser =
            await _userRepository.GetByUsernameAsync(targetUsername, ct)
            ?? throw new KeyNotFoundException($"User '{targetUsername}' not found.");

        if (targetUser.Id == ownerId)
        {
            throw new InvalidOperationException(
                "Users cannot add themselves to their own allowlist."
            );
        }

        var existing = await _privacyRepository.GetAllowlistEntryAsync(ownerId, targetUser.Id, ct);
        if (existing is not null)
        {
            return new AllowlistEntryResponse(targetUser.Uid, targetUser.Username);
        }

        var entry = InviteAllowlistEntry.Create(ownerId, targetUser.Id);
        await _privacyRepository.AddAllowlistEntryAsync(entry, ct);

        _logger.LogInformation(
            "User {OwnerId} added {TargetUsername} to invite allowlist",
            ownerId,
            targetUsername
        );

        return new AllowlistEntryResponse(targetUser.Uid, targetUser.Username);
    }

    /// <inheritdoc />
    public async Task RemoveAllowlistEntryAsync(
        int ownerId,
        Guid allowedUserUid,
        CancellationToken ct = default
    )
    {
        var entry =
            await _privacyRepository.GetAllowlistEntryByUidAsync(ownerId, allowedUserUid, ct)
            ?? throw new KeyNotFoundException(
                $"Allowlist entry for user {allowedUserUid} not found."
            );

        entry.Deactivate();
        await _privacyRepository.UpdateAllowlistEntryAsync(entry, ct);

        _logger.LogInformation(
            "User {OwnerId} removed allowlist entry for user {AllowedUserUid}",
            ownerId,
            allowedUserUid
        );
    }

    private static PrivacySettingsResponse MapToResponse(
        UserPrivacySettings settings,
        IReadOnlyList<InviteAllowlistEntry> allowlist
    )
    {
        var allowlistResponses = allowlist
            .Select(e => new AllowlistEntryResponse(e.AllowedUser.Uid, e.AllowedUser.Username))
            .ToList();

        return new PrivacySettingsResponse(
            settings.ShowPublicContent,
            settings.InvitePermission,
            allowlistResponses
        );
    }
}
