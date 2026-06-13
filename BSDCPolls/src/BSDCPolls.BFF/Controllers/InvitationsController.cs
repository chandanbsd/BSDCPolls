using BSDCPolls.BFF.Business.Invitations;
using BSDCPolls.Contracts.Requests.Invitations;
using BSDCPolls.Contracts.Responses.Notifications;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BSDCPolls.BFF.Controllers;

/// <summary>Public-facing invitation endpoints for the Angular frontend.</summary>
[ApiController]
[Authorize]
public sealed class InvitationsController : ControllerBase
{
    private readonly IBffInvitationService _invitationService;

    /// <summary>Initialises the controller with the BFF invitation service.</summary>
    public InvitationsController(IBffInvitationService invitationService)
    {
        _invitationService = invitationService;
    }

    /// <summary>Invites a user to the specified poll.</summary>
    [HttpPost("api/polls/{pollUid:guid}/invitations")]
    [ProducesResponseType(typeof(InvitationResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CreatePollInvitation(Guid pollUid, [FromBody] CreateInvitationRequest request, CancellationToken ct)
    {
        var token = ExtractBearerToken();
        if (token is null)
        {
            return Unauthorized();
        }

        var result = await _invitationService.CreatePollInvitationAsync(pollUid, request, token, ct);
        return CreatedAtAction(nameof(CreatePollInvitation), result);
    }

    /// <summary>Invites a user to the specified survey.</summary>
    [HttpPost("api/surveys/{surveyUid:guid}/invitations")]
    [ProducesResponseType(typeof(InvitationResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CreateSurveyInvitation(Guid surveyUid, [FromBody] CreateInvitationRequest request, CancellationToken ct)
    {
        var token = ExtractBearerToken();
        if (token is null)
        {
            return Unauthorized();
        }

        var result = await _invitationService.CreateSurveyInvitationAsync(surveyUid, request, token, ct);
        return CreatedAtAction(nameof(CreateSurveyInvitation), result);
    }

    private string? ExtractBearerToken()
    {
        var authHeader = HttpContext.Request.Headers.Authorization.ToString();
        if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        return authHeader["Bearer ".Length..];
    }
}
