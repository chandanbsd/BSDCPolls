using System.Security.Claims;
using BSDCPolls.Api.Business.Polls;
using BSDCPolls.Api.Data.Repositories;
using BSDCPolls.Contracts.Enums;
using BSDCPolls.Contracts.Requests.Polls;
using BSDCPolls.Contracts.Responses.Polls;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BSDCPolls.Api.Controllers;

/// <summary>Internal API endpoints for poll lifecycle, questions, voting, and results.</summary>
[ApiController]
[Route("api/internal/polls")]
[Authorize]
public sealed class PollsController : ControllerBase
{
    private readonly IPollService _pollService;
    private readonly IUserRepository _userRepository;

    /// <summary>Initialises the controller with the poll service and user repository.</summary>
    public PollsController(IPollService pollService, IUserRepository userRepository)
    {
        _pollService = pollService;
        _userRepository = userRepository;
    }

    /// <summary>Returns a paginated feed of polls visible to the current user.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(PollFeedResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetFeed(
        [FromQuery] PollStatus? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default
    )
    {
        var user = await GetCurrentUserAsync(ct);
        if (user is null)
        {
            return Unauthorized();
        }

        var result = await _pollService.GetFeedAsync(
            user.Id,
            showPublic: true,
            status,
            page,
            pageSize,
            ct
        );
        return Ok(result);
    }

    /// <summary>Creates a new poll in Draft status.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(PollDetailResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Create(
        [FromBody] CreatePollRequest request,
        CancellationToken ct
    )
    {
        var user = await GetCurrentUserAsync(ct);
        if (user is null)
        {
            return Unauthorized();
        }

        var result = await _pollService.CreateAsync(request, user.Id, ct);
        return CreatedAtAction(nameof(GetByUid), new { pollUid = result.PollUid }, result);
    }

    /// <summary>Returns full poll details including all questions and options.</summary>
    [HttpGet("{pollUid:guid}")]
    [ProducesResponseType(typeof(PollDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByUid(Guid pollUid, CancellationToken ct)
    {
        var user = await GetCurrentUserAsync(ct);
        if (user is null)
        {
            return Unauthorized();
        }

        var result = await _pollService.GetByUidAsync(pollUid, user.Id, ct);
        return Ok(result);
    }

    /// <summary>Changes the poll status (activate or close).</summary>
    [HttpPatch("{pollUid:guid}/status")]
    [ProducesResponseType(typeof(PollDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ChangeStatus(
        Guid pollUid,
        [FromBody] ChangePollStatusRequest request,
        CancellationToken ct
    )
    {
        var user = await GetCurrentUserAsync(ct);
        if (user is null)
        {
            return Unauthorized();
        }

        var result = await _pollService.ChangeStatusAsync(pollUid, request.Status, user.Id, ct);
        return Ok(result);
    }

    /// <summary>Adds a question to the poll; optionally pushes it immediately.</summary>
    [HttpPost("{pollUid:guid}/questions")]
    [ProducesResponseType(typeof(PollQuestionResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> AddQuestion(
        Guid pollUid,
        [FromBody] AddPollQuestionRequest request,
        CancellationToken ct
    )
    {
        var user = await GetCurrentUserAsync(ct);
        if (user is null)
        {
            return Unauthorized();
        }

        var result = await _pollService.AddQuestionAsync(pollUid, request, user.Id, ct);
        return Created(string.Empty, result);
    }

    /// <summary>Pushes a previously staged question to live participants.</summary>
    [HttpPost("{pollUid:guid}/questions/{questionUid:guid}/push")]
    [ProducesResponseType(typeof(PollQuestionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> PushQuestion(
        Guid pollUid,
        Guid questionUid,
        CancellationToken ct
    )
    {
        var user = await GetCurrentUserAsync(ct);
        if (user is null)
        {
            return Unauthorized();
        }

        var result = await _pollService.PushQuestionAsync(pollUid, questionUid, user.Id, ct);
        return Ok(result);
    }

    /// <summary>Submits a vote for the currently active poll question.</summary>
    [HttpPost("{pollUid:guid}/submissions")]
    [ProducesResponseType(typeof(PollSubmissionResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> SubmitVote(
        Guid pollUid,
        [FromBody] SubmitPollVoteRequest request,
        CancellationToken ct
    )
    {
        var user = await GetCurrentUserAsync(ct);
        if (user is null)
        {
            return Unauthorized();
        }

        var result = await _pollService.SubmitVoteAsync(pollUid, request, user.Id, ct);
        return Created(string.Empty, result);
    }

    /// <summary>Returns aggregated vote counts for the poll (creator only during active session).</summary>
    [HttpGet("{pollUid:guid}/results")]
    [ProducesResponseType(typeof(PollResultsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetResults(Guid pollUid, CancellationToken ct)
    {
        var user = await GetCurrentUserAsync(ct);
        if (user is null)
        {
            return Unauthorized();
        }

        var result = await _pollService.GetResultsAsync(pollUid, user.Id, ct);
        return Ok(result);
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
