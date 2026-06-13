using BSDCPolls.BFF.Business.Auth;
using BSDCPolls.BFF.Business.Privacy;
using BSDCPolls.Contracts.Requests.Privacy;
using BSDCPolls.Contracts.Responses.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BSDCPolls.BFF.Controllers;

/// <summary>User profile and privacy endpoints for the Angular frontend.</summary>
[ApiController]
[Route("api/users")]
[Authorize]
public sealed class UsersController : ControllerBase
{
    private readonly IBffAuthService _authService;
    private readonly IBffPrivacyService _privacyService;

    /// <summary>Initialises the controller with the BFF auth and privacy services.</summary>
    public UsersController(IBffAuthService authService, IBffPrivacyService privacyService)
    {
        _authService = authService;
        _privacyService = privacyService;
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

    /// <summary>Returns the current user's privacy settings and invite allowlist.</summary>
    [HttpGet("me/privacy")]
    [ProducesResponseType(typeof(PrivacySettingsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetPrivacySettings(CancellationToken ct)
    {
        var token = ExtractBearerToken();

        if (token is null)
        {
            return Unauthorized();
        }

        var result = await _privacyService.GetPrivacySettingsAsync(token, ct);
        return Ok(result);
    }

    /// <summary>Updates the current user's privacy settings.</summary>
    [HttpPut("me/privacy")]
    [ProducesResponseType(typeof(PrivacySettingsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> UpdatePrivacySettings(
        [FromBody] UpdatePrivacySettingsRequest request,
        CancellationToken ct
    )
    {
        var token = ExtractBearerToken();

        if (token is null)
        {
            return Unauthorized();
        }

        var result = await _privacyService.UpdatePrivacySettingsAsync(request, token, ct);
        return Ok(result);
    }

    /// <summary>Adds a user to the current user's invite allowlist.</summary>
    [HttpPost("me/privacy/allowlist")]
    [ProducesResponseType(typeof(AllowlistEntryResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddAllowlistEntry(
        [FromBody] AddAllowlistEntryRequest request,
        CancellationToken ct
    )
    {
        var token = ExtractBearerToken();

        if (token is null)
        {
            return Unauthorized();
        }

        var result = await _privacyService.AddAllowlistEntryAsync(request, token, ct);
        return CreatedAtAction(nameof(AddAllowlistEntry), result);
    }

    /// <summary>Removes a user from the current user's invite allowlist.</summary>
    [HttpDelete("me/privacy/allowlist/{allowedUserUid:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveAllowlistEntry(Guid allowedUserUid, CancellationToken ct)
    {
        var token = ExtractBearerToken();

        if (token is null)
        {
            return Unauthorized();
        }

        await _privacyService.RemoveAllowlistEntryAsync(allowedUserUid, token, ct);
        return NoContent();
    }

    private string? ExtractBearerToken()
    {
        var authHeader = HttpContext.Request.Headers.Authorization.ToString();

        if (
            string.IsNullOrEmpty(authHeader)
            || !authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)
        )
        {
            return null;
        }

        return authHeader["Bearer ".Length..].Trim();
    }
}
