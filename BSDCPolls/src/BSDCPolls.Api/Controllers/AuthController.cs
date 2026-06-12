using System.Security.Claims;
using BSDCPolls.Api.Business.Auth;
using BSDCPolls.Contracts.Requests.Auth;
using BSDCPolls.Contracts.Responses.Auth;
using BSDCPolls.Contracts.Responses.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BSDCPolls.Api.Controllers;

/// <summary>Handles user registration, login, and username management for the internal API.</summary>
[ApiController]
[Route("api/internal/auth")]
public sealed class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    /// <summary>Initialises the controller with the auth service.</summary>
    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    /// <summary>Registers a new user account and returns the generated username and user UID.</summary>
    [AllowAnonymous]
    [HttpPost("register")]
    [ProducesResponseType(typeof(RegisterResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register(
        [FromBody] RegisterRequest request,
        CancellationToken ct)
    {
        var result = await _authService.RegisterAsync(request, ct);
        return CreatedAtAction(nameof(Register), result);
    }

    /// <summary>Authenticates a user by username and password, returning a JWT access token.</summary>
    [AllowAnonymous]
    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login(
        [FromBody] LoginRequest request,
        CancellationToken ct)
    {
        var result = await _authService.LoginAsync(request, ct);
        return Ok(result);
    }

    /// <summary>Generates a new random username for the authenticated user, subject to rate limits.</summary>
    [Authorize]
    [HttpPost("change-username")]
    [ProducesResponseType(typeof(UsernameChangeResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> ChangeUsername(CancellationToken ct)
    {
        // GoTrue JWTs carry the synthetic email in the 'email' claim, which equals SupabaseUserId.
        var supabaseUserId = User.FindFirstValue("email");

        if (string.IsNullOrEmpty(supabaseUserId))
        {
            return Unauthorized();
        }

        var newUsername = await _authService.ChangeUsernameAsync(supabaseUserId, ct);
        return Ok(new UsernameChangeResponse(newUsername));
    }
}
