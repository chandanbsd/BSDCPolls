using BSDCPolls.BFF.Business.Surveys;
using BSDCPolls.Contracts.Enums;
using BSDCPolls.Contracts.Requests.Surveys;
using BSDCPolls.Contracts.Responses.Surveys;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BSDCPolls.BFF.Controllers;

/// <summary>Public-facing survey endpoints for the Angular frontend.</summary>
[ApiController]
[Route("api/surveys")]
[Authorize]
public sealed class SurveysController : ControllerBase
{
    private readonly IBffSurveyService _surveyService;

    /// <summary>Initialises the controller with the BFF survey service.</summary>
    public SurveysController(IBffSurveyService surveyService)
    {
        _surveyService = surveyService;
    }

    /// <summary>Returns a paginated feed of surveys visible to the current user.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(SurveyFeedResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetFeed(
        [FromQuery] SurveyStatus? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default
    )
    {
        var token = ExtractBearerToken();
        if (token is null)
        {
            return Unauthorized();
        }

        var result = await _surveyService.GetFeedAsync(status, page, pageSize, token, ct);
        return Ok(result);
    }

    /// <summary>Creates a new survey in Draft status.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(SurveyDetailResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Create(
        [FromBody] CreateSurveyRequest request,
        CancellationToken ct
    )
    {
        var token = ExtractBearerToken();
        if (token is null)
        {
            return Unauthorized();
        }

        var result = await _surveyService.CreateAsync(request, token, ct);
        return CreatedAtAction(nameof(GetByUid), new { surveyUid = result.SurveyUid }, result);
    }

    /// <summary>Returns full survey details including the question tree.</summary>
    [HttpGet("{surveyUid:guid}")]
    [ProducesResponseType(typeof(SurveyDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByUid(Guid surveyUid, CancellationToken ct)
    {
        var token = ExtractBearerToken();
        if (token is null)
        {
            return Unauthorized();
        }

        var result = await _surveyService.GetByUidAsync(surveyUid, token, ct);
        return Ok(result);
    }

    /// <summary>Changes the survey status (publish or close).</summary>
    [HttpPatch("{surveyUid:guid}/status")]
    [ProducesResponseType(typeof(SurveyDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ChangeStatus(
        Guid surveyUid,
        [FromBody] ChangeSurveyStatusRequest request,
        CancellationToken ct
    )
    {
        var token = ExtractBearerToken();
        if (token is null)
        {
            return Unauthorized();
        }

        var result = await _surveyService.ChangeStatusAsync(surveyUid, request, token, ct);
        return Ok(result);
    }

    /// <summary>Replaces the question tree on a Draft survey.</summary>
    [HttpPut("{surveyUid:guid}/questions")]
    [ProducesResponseType(typeof(SurveyDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> UpdateQuestions(
        Guid surveyUid,
        [FromBody] UpdateSurveyQuestionsRequest request,
        CancellationToken ct
    )
    {
        var token = ExtractBearerToken();
        if (token is null)
        {
            return Unauthorized();
        }

        var result = await _surveyService.UpdateQuestionsAsync(surveyUid, request, token, ct);
        return Ok(result);
    }

    /// <summary>Saves progress or submits a survey response.</summary>
    [HttpPost("{surveyUid:guid}/responses")]
    [ProducesResponseType(typeof(SurveyResponseStatusResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> SaveResponse(
        Guid surveyUid,
        [FromBody] SaveSurveyResponseRequest request,
        CancellationToken ct
    )
    {
        var token = ExtractBearerToken();
        if (token is null)
        {
            return Unauthorized();
        }

        var result = await _surveyService.SaveResponseAsync(surveyUid, request, token, ct);
        return Ok(result);
    }

    /// <summary>Uploads a PDF for a document-upload question. Validates PDF content-type and 10 MB limit at BFF edge.</summary>
    [HttpPost("{surveyUid:guid}/responses/{responseUid:guid}/documents")]
    [ProducesResponseType(typeof(SurveyDocumentResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> UploadDocument(
        Guid surveyUid,
        Guid responseUid,
        [FromForm] IFormFile file,
        [FromForm] Guid questionUid,
        CancellationToken ct
    )
    {
        var token = ExtractBearerToken();
        if (token is null)
        {
            return Unauthorized();
        }

        if (!string.Equals(file.ContentType, "application/pdf", StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest(new { message = "Only PDF files are accepted." });
        }

        const long maxBytes = 10 * 1024 * 1024;
        if (file.Length > maxBytes)
        {
            return BadRequest(new { message = "File size exceeds the 10 MB limit." });
        }

        using var stream = file.OpenReadStream();
        var result = await _surveyService.UploadDocumentAsync(
            surveyUid,
            responseUid,
            stream,
            file.FileName,
            file.ContentType,
            file.Length,
            questionUid,
            token,
            ct
        );

        return Created(string.Empty, result);
    }

    /// <summary>Returns aggregated survey results (creator only).</summary>
    [HttpGet("{surveyUid:guid}/results")]
    [ProducesResponseType(typeof(SurveyResultsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetResults(Guid surveyUid, CancellationToken ct)
    {
        var token = ExtractBearerToken();
        if (token is null)
        {
            return Unauthorized();
        }

        var result = await _surveyService.GetResultsAsync(surveyUid, token, ct);
        return Ok(result);
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
