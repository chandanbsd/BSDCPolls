using System.Security.Claims;
using BSDCPolls.Api.Business.Invitations;
using BSDCPolls.Api.Data.Repositories;
using BSDCPolls.Contracts.Requests.Invitations;
using BSDCPolls.Contracts.Responses.Notifications;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BSDCPolls.Api.Controllers;

/// <summary>Internal API endpoints for creating poll and survey invitations.</summary>
[ApiController]
[Route("api/internal")]
[Authorize]
public sealed class InvitationsController : ControllerBase
{
    private readonly IInvitationService _invitationService;
    private readonly IUserRepository _userRepository;

    /// <summary>Initialises the controller with the invitation service and user repository.</summary>
    public InvitationsController(IInvitationService invitationService, IUserRepository userRepository)
    {
        _invitationService = invitationService;
        _userRepository = userRepository;
    }

    /// <summary>Invites a user to the specified poll.</summary>
    [HttpPost("polls/{pollUid:guid}/invitations")]
    [ProducesResponseType(typeof(InvitationResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CreatePollInvitation(Guid pollUid, [FromBody] CreateInvitationRequest request, CancellationToken ct)
    {
        var user = await GetCurrentUserAsync(ct);
        if (user is null)
        {
            return Unauthorized();
        }

        var result = await _invitationService.CreatePollInvitationAsync(pollUid, request.TargetUsername, user.Id, ct);
        return CreatedAtAction(nameof(CreatePollInvitation), result);
    }

    /// <summary>Invites a user to the specified survey.</summary>
    [HttpPost("surveys/{surveyUid:guid}/invitations")]
    [ProducesResponseType(typeof(InvitationResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CreateSurveyInvitation(Guid surveyUid, [FromBody] CreateInvitationRequest request, CancellationToken ct)
    {
        var user = await GetCurrentUserAsync(ct);
        if (user is null)
        {
            return Unauthorized();
        }

        var result = await _invitationService.CreateSurveyInvitationAsync(surveyUid, request.TargetUsername, user.Id, ct);
        return CreatedAtAction(nameof(CreateSurveyInvitation), result);
    }

    private async Task<BSDCPolls.Api.Data.Entities.ApplicationUser?> GetCurrentUserAsync(CancellationToken ct)
    {
        var supabaseUserId = User.FindFirstValue("email");
        if (string.IsNullOrEmpty(supabaseUserId))
        {
            return null;
        }

        return await _userRepository.GetBySupabaseIdAsync(supabaseUserId, ct);
    }
}
