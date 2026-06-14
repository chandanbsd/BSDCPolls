using BSDCPolls.Api.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace BSDCPolls.Api.Data.Repositories;

/// <summary>EF Core implementation of <see cref="IPrivacySettingsRepository"/>.</summary>
public sealed class PrivacySettingsRepository : IPrivacySettingsRepository
{
    private readonly BsdcPollsDbContext _db;

    /// <summary>Initialises the repository with the scoped <see cref="BsdcPollsDbContext"/>.</summary>
    public PrivacySettingsRepository(BsdcPollsDbContext db)
    {
        _db = db;
    }

    /// <inheritdoc />
    public Task<UserPrivacySettings?> GetByUserIdAsync(
        int userId,
        CancellationToken ct = default
    ) =>
        _db
            .UserPrivacySettings.Include(p => p.User)
            .FirstOrDefaultAsync(p => p.UserId == userId && p.IsActive, ct);

    /// <inheritdoc />
    public async Task<UserPrivacySettings> CreateAsync(
        UserPrivacySettings settings,
        CancellationToken ct = default
    )
    {
        _db.UserPrivacySettings.Add(settings);
        await _db.SaveChangesAsync(ct);
        return settings;
    }

    /// <inheritdoc />
    public Task UpdateAsync(UserPrivacySettings settings, CancellationToken ct = default) =>
        _db.SaveChangesAsync(ct).ContinueWith(_ => { }, ct);

    /// <inheritdoc />
    public Task<IReadOnlyList<InviteAllowlistEntry>> GetAllowlistAsync(
        int ownerId,
        CancellationToken ct = default
    ) =>
        _db
            .InviteAllowlistEntries.Include(e => e.AllowedUser)
            .Where(e => e.OwnerId == ownerId && e.IsActive)
            .OrderBy(e => e.AllowedUser.Username)
            .ToListAsync(ct)
            .ContinueWith(t => (IReadOnlyList<InviteAllowlistEntry>)t.Result, ct);

    /// <inheritdoc />
    public Task<InviteAllowlistEntry?> GetAllowlistEntryAsync(
        int ownerId,
        int allowedUserId,
        CancellationToken ct = default
    ) =>
        _db.InviteAllowlistEntries.FirstOrDefaultAsync(
            e => e.OwnerId == ownerId && e.AllowedUserId == allowedUserId && e.IsActive,
            ct
        );

    /// <inheritdoc />
    public async Task<InviteAllowlistEntry> AddAllowlistEntryAsync(
        InviteAllowlistEntry entry,
        CancellationToken ct = default
    )
    {
        _db.InviteAllowlistEntries.Add(entry);
        await _db.SaveChangesAsync(ct);
        return entry;
    }

    /// <inheritdoc />
    public Task<InviteAllowlistEntry?> GetAllowlistEntryByUidAsync(
        int ownerId,
        Guid allowedUserUid,
        CancellationToken ct = default
    ) =>
        _db
            .InviteAllowlistEntries.Include(e => e.AllowedUser)
            .FirstOrDefaultAsync(
                e => e.OwnerId == ownerId && e.AllowedUser.Uid == allowedUserUid && e.IsActive,
                ct
            );

    /// <inheritdoc />
    public Task UpdateAllowlistEntryAsync(
        InviteAllowlistEntry entry,
        CancellationToken ct = default
    ) => _db.SaveChangesAsync(ct).ContinueWith(_ => { }, ct);
}
