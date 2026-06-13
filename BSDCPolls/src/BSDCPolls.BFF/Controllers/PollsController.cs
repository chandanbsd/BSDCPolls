using BSDCPolls.BFF.Business.Polls;
using BSDCPolls.Contracts.Enums;
using BSDCPolls.Contracts.Requests.Polls;
using BSDCPolls.Contracts.Responses.Polls;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BSDCPolls.BFF.Controllers;

/// <summary>Public-facing poll endpoints for the Angular frontend.</summary>
[ApiController]
[Route("api/polls")]
[Authorize]
public sealed class PollsController : ControllerBase
{
    private readonly IBffPollService _pollService;

    /// <summary>Initialises the controller with the BFF poll service.</summary>
    public PollsController(IBffPollService pollService)
    {
        _pollService = pollService;
    }

    /// <summary>Returns a paginated feed of polls visible to the current user.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(PollFeedResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetFeed(
        [FromQuery] PollStatus? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var token = ExtractBearerToken();
        if (token is null)
        {
            return Unauthorized();
        }

        var result = await _pollService.GetFeedAsync(status, page, pageSize, token, ct);
        return Ok(result);
    }

    /// <summary>Creates a new poll in Draft status.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(PollDetailResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Create([FromBody] CreatePollRequest request, CancellationToken ct)
    {
        var token = ExtractBearerToken();
        if (token is null)
        {
            return Unauthorized();
        }

        var result = await _pollService.CreateAsync(request, token, ct);
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
        var token = ExtractBearerToken();
        if (token is null)
        {
            return Unauthorized();
        }

        var result = await _pollService.GetByUidAsync(pollUid, token, ct);
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
        CancellationToken ct)
    {
        var token = ExtractBearerToken();
        if (token is null)
        {
            return Unauthorized();
        }

        var result = await _pollService.ChangeStatusAsync(pollUid, request, token, ct);
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
        CancellationToken ct)
    {
        var token = ExtractBearerToken();
        if (token is null)
        {
            return Unauthorized();
        }

        var result = await _pollService.AddQuestionAsync(pollUid, request, token, ct);
        return Created(string.Empty, result);
    }

    /// <summary>Pushes a previously staged question to live participants.</summary>
    [HttpPost("{pollUid:guid}/questions/{questionUid:guid}/push")]
    [ProducesResponseType(typeof(PollQuestionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> PushQuestion(Guid pollUid, Guid questionUid, CancellationToken ct)
    {
        var token = ExtractBearerToken();
        if (token is null)
        {
            return Unauthorized();
        }

        var result = await _pollService.PushQuestionAsync(pollUid, questionUid, token, ct);
        return Ok(result);
    }

    /// <summary>Returns aggregated vote counts for the poll.</summary>
    [HttpGet("{pollUid:guid}/results")]
    [ProducesResponseType(typeof(PollResultsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetResults(Guid pollUid, CancellationToken ct)
    {
        var token = ExtractBearerToken();
        if (token is null)
        {
            return Unauthorized();
        }

        var result = await _pollService.GetResultsAsync(pollUid, token, ct);
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
