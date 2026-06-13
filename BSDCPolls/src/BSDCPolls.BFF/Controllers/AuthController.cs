using BSDCPolls.BFF.Business.Auth;
using BSDCPolls.Contracts.Requests.Auth;
using BSDCPolls.Contracts.Responses.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BSDCPolls.BFF.Controllers;

/// <summary>Public-facing auth endpoints for the Angular frontend.</summary>
[ApiController]
[Route("api/auth")]
public sealed class AuthController : ControllerBase
{
    private readonly IBffAuthService _authService;

    /// <summary>Initialises the controller with the BFF auth service.</summary>
    public AuthController(IBffAuthService authService)
    {
        _authService = authService;
    }

    /// <summary>Registers a new user account and returns the auto-generated username and user UID.</summary>
    [AllowAnonymous]
    [HttpPost("register")]
    [ProducesResponseType(typeof(RegisterResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register(
        [FromBody] RegisterRequest request,
        CancellationToken ct
    )
    {
        var result = await _authService.RegisterAsync(request, ct);
        return CreatedAtAction(nameof(Register), result);
    }

    /// <summary>Authenticates a user by username and password, returning a JWT access token.</summary>
    [AllowAnonymous]
    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken ct)
    {
        var result = await _authService.LoginAsync(request, ct);
        return Ok(result);
    }
}
