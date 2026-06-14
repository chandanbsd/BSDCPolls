namespace BSDCPolls.Api.Data.Infrastructure;

/// <summary>
/// Provides the internal integer user ID of the currently authenticated user
/// to the EF Core <c>AuditInterceptor</c>.
/// </summary>
public interface ICurrentUserContext
{
    /// <summary>
    /// Returns the internal <c>ApplicationUser.Id</c> for the active request.
    /// Returns <c>1</c> (the system sentinel) when no authenticated user is in scope
    /// (e.g., during initial registration or background jobs).
    /// </summary>
    int UserId { get; }
}
