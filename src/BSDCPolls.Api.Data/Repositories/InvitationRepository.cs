using BSDCPolls.Api.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace BSDCPolls.Api.Data.Repositories;

/// <summary>EF Core implementation of <see cref="IInvitationRepository"/>.</summary>
public sealed class InvitationRepository : IInvitationRepository
{
    private readonly BsdcPollsDbContext _db;

    /// <summary>Initialises the repository with the scoped <see cref="BsdcPollsDbContext"/>.</summary>
    public InvitationRepository(BsdcPollsDbContext db)
    {
        _db = db;
    }

    /// <inheritdoc />
    public async Task<Invitation> CreateAsync(Invitation invitation, CancellationToken ct = default)
    {
        _db.Invitations.Add(invitation);
        await _db.SaveChangesAsync(ct);
        return invitation;
    }

    /// <inheritdoc />
    public Task<Invitation?> GetByUidAsync(Guid uid, CancellationToken ct = default) =>
        _db
            .Invitations.Include(i => i.Inviter)
            .Include(i => i.Invitee)
            .Include(i => i.Poll)
            .Include(i => i.Survey)
            .FirstOrDefaultAsync(i => i.Uid == uid && i.IsActive, ct);

    /// <inheritdoc />
    public Task<bool> IsDuplicateAsync(
        int inviteeId,
        int? pollId,
        int? surveyId,
        CancellationToken ct = default
    ) =>
        _db.Invitations.AnyAsync(
            i =>
                i.InviteeId == inviteeId
                && i.IsActive
                && (pollId.HasValue ? i.PollId == pollId : i.SurveyId == surveyId),
            ct
        );

    /// <inheritdoc />
    public Task<Invitation?> GetForPollAsync(
        Guid pollUid,
        int inviteeId,
        CancellationToken ct = default
    ) =>
        _db.Invitations.FirstOrDefaultAsync(
            i => i.Poll != null && i.Poll.Uid == pollUid && i.InviteeId == inviteeId && i.IsActive,
            ct
        );

    /// <inheritdoc />
    public Task<Invitation?> GetForSurveyAsync(
        Guid surveyUid,
        int inviteeId,
        CancellationToken ct = default
    ) =>
        _db.Invitations.FirstOrDefaultAsync(
            i =>
                i.Survey != null
                && i.Survey.Uid == surveyUid
                && i.InviteeId == inviteeId
                && i.IsActive,
            ct
        );
}
