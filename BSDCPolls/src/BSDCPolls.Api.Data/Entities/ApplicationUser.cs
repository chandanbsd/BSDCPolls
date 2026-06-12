using System.ComponentModel.DataAnnotations;

namespace BSDCPolls.Api.Data.Entities;

/// <summary>
/// Represents a registered BSDCPolls user. Stores only the system-generated username
/// and a reference to the Supabase GoTrue identity. No PII is ever stored.
/// </summary>
public class ApplicationUser : AuditableEntity
{
    /// <summary>System-generated three-word hyphen-separated username. Unique across the system.</summary>
    [Required]
    [MaxLength(60)]
    public string Username { get; private set; } = string.Empty;

    /// <summary>
    /// Subject claim (<c>sub</c>) from Supabase GoTrue.
    /// Internally this is the synthetic email used for GoTrue registration — never surfaced to users.
    /// </summary>
    [Required]
    [MaxLength(200)]
    public string SupabaseUserId { get; private set; } = string.Empty;

    /// <summary>EF Core proxy constructor. Do not use in application code.</summary>
    protected ApplicationUser()
    {
    }

    private ApplicationUser(string username, string supabaseUserId)
    {
        InitialiseIdentity(Guid.NewGuid());
        Username = username;
        SupabaseUserId = supabaseUserId;
    }

    /// <summary>
    /// Creates a new ApplicationUser with a system-generated username.
    /// CreatedById and UpdatedById are set to the system sentinel user (Id=1)
    /// by the <c>AuditInterceptor</c> before the INSERT is persisted.
    /// </summary>
    /// <param name="username">A unique, profanity-free three-word username.</param>
    /// <param name="supabaseUserId">The Supabase GoTrue subject identifier for this user.</param>
    /// <returns>A new <see cref="ApplicationUser"/> instance ready for persistence.</returns>
    public static ApplicationUser Create(string username, string supabaseUserId)
    {
        if (string.IsNullOrWhiteSpace(username))
        {
            throw new ArgumentException("Username must not be empty.", nameof(username));
        }

        if (string.IsNullOrWhiteSpace(supabaseUserId))
        {
            throw new ArgumentException("Supabase user ID must not be empty.", nameof(supabaseUserId));
        }

        return new ApplicationUser(username, supabaseUserId);
    }

    /// <summary>
    /// Replaces the current username with a newly generated one.
    /// The caller is responsible for persisting the previous username to <c>UsernameHistory</c>.
    /// </summary>
    /// <param name="newUsername">A unique, profanity-free three-word username.</param>
    public void UpdateUsername(string newUsername)
    {
        if (string.IsNullOrWhiteSpace(newUsername))
        {
            throw new ArgumentException("New username must not be empty.", nameof(newUsername));
        }

        Username = newUsername;
    }

    /// <summary>Soft-deletes this user, preventing future logins.</summary>
    public void Deactivate()
    {
        MarkInactive();
    }
}
