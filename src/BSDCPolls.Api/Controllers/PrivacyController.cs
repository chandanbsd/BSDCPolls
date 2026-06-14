using System.Security.Claims;
using BSDCPolls.Api.Business.Privacy;
using BSDCPolls.Api.Data.Repositories;
using BSDCPolls.Contracts.Requests.Privacy;
using BSDCPolls.Contracts.Responses.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BSDCPolls.Api.Controllers;

/// <summary>Internal API endpoints for managing user privacy settings and invite allowlists.</summary>
[ApiController]
[Route("api/internal/users/me")]
[Authorize]
public sealed class PrivacyController : ControllerBase
{
    private readonly IPrivacyService _privacyService;
    private readonly IUserRepository _userRepository;

    /// <summary>Initialises the controller with the privacy service and user repository.</summary>
    public PrivacyController(IPrivacyService privacyService, IUserRepository userRepository)
    {
        _privacyService = privacyService;
        _userRepository = userRepository;
    }

    /// <summary>Returns the current user's privacy settings and invite allowlist.</summary>
    [HttpGet("privacy")]
    [ProducesResponseType(typeof(PrivacySettingsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetPrivacySettings(CancellationToken ct)
    {
        var user = await GetCurrentUserAsync(ct);
        if (user is null)
        {
            return Unauthorized();
        }

        var result = await _privacyService.GetSettingsAsync(user.Id, ct);
        return Ok(result);
    }

    /// <summary>Updates the current user's privacy settings.</summary>
    [HttpPut("privacy")]
    [ProducesResponseType(typeof(PrivacySettingsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> UpdatePrivacySettings(
        [FromBody] UpdatePrivacySettingsRequest request,
        CancellationToken ct
    )
    {
        var user = await GetCurrentUserAsync(ct);
        if (user is null)
        {
            return Unauthorized();
        }

        var result = await _privacyService.UpdateSettingsAsync(user.Id, request, ct);
        return Ok(result);
    }

    /// <summary>Adds a user to the current user's invite allowlist.</summary>
    [HttpPost("privacy/allowlist")]
    [ProducesResponseType(typeof(AllowlistEntryResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddAllowlistEntry(
        [FromBody] AddAllowlistEntryRequest request,
        CancellationToken ct
    )
    {
        var user = await GetCurrentUserAsync(ct);
        if (user is null)
        {
            return Unauthorized();
        }

        var result = await _privacyService.AddAllowlistEntryAsync(user.Id, request.Username, ct);
        return CreatedAtAction(nameof(AddAllowlistEntry), result);
    }

    /// <summary>Removes an entry from the current user's invite allowlist.</summary>
    [HttpDelete("privacy/allowlist/{allowedUserUid:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveAllowlistEntry(Guid allowedUserUid, CancellationToken ct)
    {
        var user = await GetCurrentUserAsync(ct);
        if (user is null)
        {
            return Unauthorized();
        }

        await _privacyService.RemoveAllowlistEntryAsync(user.Id, allowedUserUid, ct);
        return NoContent();
    }

    private async Task<BSDCPolls.Api.Data.Entities.ApplicationUser?> GetCurrentUserAsync(
        CancellationToken ct
    )
    {
        var supabaseUserId = User.FindFirstValue("email");
        if (string.IsNullOrEmpty(supabaseUserId))
        {
            return null;
        }

        return await _userRepository.GetBySupabaseIdAsync(supabaseUserId, ct);
    }
}
