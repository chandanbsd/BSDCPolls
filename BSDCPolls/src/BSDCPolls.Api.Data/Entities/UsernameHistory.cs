using System.ComponentModel.DataAnnotations;

namespace BSDCPolls.Api.Data.Entities;

/// <summary>
/// Tracks previously used usernames to prevent reuse within 90 days.
/// Entries are immutable once created.
/// </summary>
public class UsernameHistory : AuditableEntity
{
    /// <summary>FK to the user whose username history this records.</summary>
    public int UserId { get; private set; }

    /// <summary>Navigation property to the owning user.</summary>
    public virtual ApplicationUser User { get; private set; } = null!;

    /// <summary>The former username, retained for reuse prevention.</summary>
    [Required]
    [MaxLength(60)]
    public string Username { get; private set; } = string.Empty;

    /// <summary>UTC timestamp when this username was retired.</summary>
    public DateTime RetiredAt { get; private set; }

    /// <summary>EF Core proxy constructor.</summary>
    protected UsernameHistory()
    {
    }

    private UsernameHistory(int userId, string username, DateTime retiredAt)
    {
        InitialiseIdentity(Guid.NewGuid());
        UserId = userId;
        Username = username;
        RetiredAt = retiredAt;
    }

    /// <summary>
    /// Records that a user retired the given username at the specified time.
    /// </summary>
    /// <param name="userId">The internal ID of the user who owned this username.</param>
    /// <param name="username">The username being retired.</param>
    /// <param name="retiredAt">The UTC time at which the username was replaced.</param>
    /// <returns>A new <see cref="UsernameHistory"/> record ready for persistence.</returns>
    public static UsernameHistory Create(int userId, string username, DateTime retiredAt)
    {
        if (string.IsNullOrWhiteSpace(username))
            throw new ArgumentException("Username must not be empty.", nameof(username));

        return new UsernameHistory(userId, username, retiredAt);
    }
}
