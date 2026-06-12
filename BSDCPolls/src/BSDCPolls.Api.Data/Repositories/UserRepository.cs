using BSDCPolls.Api.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace BSDCPolls.Api.Data.Repositories;

/// <summary>EF Core implementation of <see cref="IUserRepository"/>.</summary>
public sealed class UserRepository : IUserRepository
{
    private readonly BsdcPollsDbContext _db;

    /// <summary>Initialises the repository with the scoped <see cref="BsdcPollsDbContext"/>.</summary>
    public UserRepository(BsdcPollsDbContext db)
    {
        _db = db;
    }

    /// <inheritdoc />
    public Task<ApplicationUser?> GetBySupabaseIdAsync(string supabaseUserId, CancellationToken ct = default) =>
        _db.ApplicationUsers
            .FirstOrDefaultAsync(u => u.SupabaseUserId == supabaseUserId && u.IsActive, ct);

    /// <inheritdoc />
    public Task<ApplicationUser?> GetByUsernameAsync(string username, CancellationToken ct = default) =>
        _db.ApplicationUsers
            .FirstOrDefaultAsync(u => u.Username == username && u.IsActive, ct);

    /// <inheritdoc />
    public Task<ApplicationUser?> GetByUidAsync(Guid uid, CancellationToken ct = default) =>
        _db.ApplicationUsers
            .FirstOrDefaultAsync(u => u.Uid == uid && u.IsActive, ct);

    /// <inheritdoc />
    public Task<bool> UsernameExistsAsync(string username, CancellationToken ct = default) =>
        _db.ApplicationUsers
            .AnyAsync(u => u.Username == username && u.IsActive, ct);

    /// <inheritdoc />
    public async Task<ApplicationUser> CreateAsync(ApplicationUser user, CancellationToken ct = default)
    {
        _db.ApplicationUsers.Add(user);
        await _db.SaveChangesAsync(ct);
        return user;
    }

    /// <inheritdoc />
    public Task UpdateAsync(ApplicationUser user, CancellationToken ct = default) =>
        _db.SaveChangesAsync(ct).ContinueWith(_ => { }, ct);
}
