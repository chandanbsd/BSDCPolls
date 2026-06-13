namespace BSDCPolls.BFF.Controllers;

/// <summary>Payload sent by the Angular <c>GlobalErrorHandler</c> when an unhandled error occurs.</summary>
/// <param name="Message">Human-readable error message.</param>
/// <param name="Stack">JavaScript stack trace, if available.</param>
/// <param name="Route">Angular route URL where the error occurred.</param>
/// <param name="Component">Angular component context, if available.</param>
public sealed record ClientErrorReport(
    string? Message,
    string? Stack,
    string? Route,
    string? Component
);
