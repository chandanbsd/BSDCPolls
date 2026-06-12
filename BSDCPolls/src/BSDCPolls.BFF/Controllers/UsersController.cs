using BSDCPolls.BFF.Business.Auth;
using BSDCPolls.Contracts.Responses.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BSDCPolls.BFF.Controllers;

/// <summary>User profile endpoints for the Angular frontend.</summary>
[ApiController]
[Route("api/users")]
[Authorize]
public sealed class UsersController : ControllerBase
{
    private readonly IBffAuthService _authService;

    /// <summary>Initialises the controller with the BFF auth service.</summary>
    public UsersController(IBffAuthService authService)
    {
        _authService = authService;
    }

    /// <summary>Returns the profile of the currently authenticated user.</summary>
    [HttpGet("me")]
    [ProducesResponseType(typeof(UserProfileResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetMe(CancellationToken ct)
    {
        var token = ExtractBearerToken();

        if (token is null)
        {
            return Unauthorized();
        }

        var profile = await _authService.GetProfileAsync(token, ct);
        return Ok(profile);
    }

    /// <summary>Generates a new random username for the authenticated user, subject to rate limits.</summary>
    [HttpPost("me/username/change")]
    [ProducesResponseType(typeof(UsernameChangeResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> ChangeUsername(CancellationToken ct)
    {
        var token = ExtractBearerToken();

        if (token is null)
        {
            return Unauthorized();
        }

        var result = await _authService.ChangeUsernameAsync(token, ct);
        return Ok(result);
    }

    private string? ExtractBearerToken()
    {
        var authHeader = HttpContext.Request.Headers.Authorization.ToString();

        if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        return authHeader["Bearer ".Length..].Trim();
    }
}
