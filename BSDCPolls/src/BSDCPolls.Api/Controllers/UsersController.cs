using System.Security.Claims;
using BSDCPolls.Api.Data.Repositories;
using BSDCPolls.Contracts.Responses.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BSDCPolls.Api.Controllers;

/// <summary>Internal API endpoints for user profile operations.</summary>
[ApiController]
[Route("api/internal/users")]
[Authorize]
public sealed class UsersController : ControllerBase
{
    private readonly IUserRepository _userRepository;

    /// <summary>Initialises the controller with the user repository.</summary>
    public UsersController(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    /// <summary>Returns the profile of the currently authenticated user.</summary>
    [HttpGet("me")]
    [ProducesResponseType(typeof(UserProfileResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetMe(CancellationToken ct)
    {
        // GoTrue JWTs carry the synthetic email in the 'email' claim, which equals SupabaseUserId.
        var supabaseUserId = User.FindFirstValue("email");

        if (string.IsNullOrEmpty(supabaseUserId))
        {
            return Unauthorized();
        }

        var user = await _userRepository.GetBySupabaseIdAsync(supabaseUserId, ct);

        if (user is null)
        {
            return Unauthorized();
        }

        return Ok(new UserProfileResponse(user.Uid, user.Username, user.CreatedOn));
    }
}
