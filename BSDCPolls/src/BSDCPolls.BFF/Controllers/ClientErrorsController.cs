using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BSDCPolls.BFF.Controllers;

/// <summary>Receives client-side error reports from the Angular frontend's global error handler.</summary>
[ApiController]
[Route("api/client-errors")]
[AllowAnonymous]
public sealed class ClientErrorsController : ControllerBase
{
    private readonly ILogger<ClientErrorsController> _logger;

    /// <summary>Initialises the controller with the logger.</summary>
    public ClientErrorsController(ILogger<ClientErrorsController> logger)
    {
        _logger = logger;
    }

    /// <summary>Accepts an unhandled client-side error report and logs it as a structured error.</summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public IActionResult ReportClientError([FromBody] ClientErrorReport report)
    {
        _logger.LogError(
            "Client-side error — Route: {Route} | Component: {Component} | Message: {Message} | Stack: {Stack}",
            report.Route,
            report.Component,
            report.Message,
            report.Stack
        );

        return NoContent();
    }
}
