using Microsoft.AspNetCore.Http;

namespace BSDCPolls.Api.Data.Infrastructure;

/// <summary>
/// HTTP request-scoped implementation of <see cref="ICurrentUserContext"/>.
/// Reads the user ID from the claim injected by <c>AuditUserMiddleware</c> after
/// the Supabase JWT has been validated and the internal user ID has been resolved.
/// Falls back to the system sentinel (Id=1) when no claim is present.
/// </summary>
public sealed class CurrentUserContext : ICurrentUserContext
{
    private const int SystemSentinelId = 1;
    private const string UserIdClaimType = "bsdcpolls:userid";

    private readonly IHttpContextAccessor _httpContextAccessor;

    /// <summary>Initialises a new instance of <see cref="CurrentUserContext"/>.</summary>
    /// <param name="httpContextAccessor">Accessor to the current HTTP request context.</param>
    public CurrentUserContext(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    /// <inheritdoc />
    public int UserId
    {
        get
        {
            var claim = _httpContextAccessor.HttpContext?.User.FindFirst(UserIdClaimType);
            if (claim is not null && int.TryParse(claim.Value, out var userId))
                return userId;

            return SystemSentinelId;
        }
    }
}
