using BSDCPolls.Api.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace BSDCPolls.Api.Data.Repositories;

/// <summary>EF Core implementation of <see cref="IUsernameHistoryRepository"/>.</summary>
public sealed class UsernameHistoryRepository : IUsernameHistoryRepository
{
    private readonly BsdcPollsDbContext _db;

    /// <summary>Initialises the repository with the scoped <see cref="BsdcPollsDbContext"/>.</summary>
    public UsernameHistoryRepository(BsdcPollsDbContext db)
    {
        _db = db;
    }

    /// <inheritdoc />
    public async Task AddAsync(UsernameHistory entry, CancellationToken ct = default)
    {
        _db.UsernameHistories.Add(entry);
        await _db.SaveChangesAsync(ct);
    }

    /// <inheritdoc />
    public Task<bool> IsUsernameRecentlyUsedAsync(
        string username,
        int userId,
        int days,
        CancellationToken ct = default
    )
    {
        var cutoff = DateTime.UtcNow.AddDays(-days);
        return _db.UsernameHistories.AnyAsync(
            h => h.UserId == userId && h.Username == username && h.RetiredAt >= cutoff,
            ct
        );
    }

    /// <inheritdoc />
    public Task<int> CountRecentChangesAsync(int userId, int days, CancellationToken ct = default)
    {
        var cutoff = DateTime.UtcNow.AddDays(-days);
        return _db.UsernameHistories.CountAsync(
            h => h.UserId == userId && h.RetiredAt >= cutoff,
            ct
        );
    }
}
