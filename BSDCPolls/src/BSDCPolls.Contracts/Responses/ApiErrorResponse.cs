namespace BSDCPolls.Contracts.Responses;

/// <summary>
/// Standard error response returned by both BFF and backend API on all error conditions.
/// Never includes stack traces, exception type names, or internal service identifiers.
/// </summary>
/// <param name="TraceId">W3C trace ID from the current OpenTelemetry span; the only bridge between a user-reported error and the SigNoz trace.</param>
/// <param name="Status">HTTP status code mirrored in the response body.</param>
/// <param name="Message">User-readable error message safe for display in the UI.</param>
/// <param name="Errors">Optional per-field validation errors; empty for non-validation failures.</param>
public sealed record ApiErrorResponse(
    string TraceId,
    int Status,
    string Message,
    IReadOnlyList<ApiFieldError>? Errors = null);
