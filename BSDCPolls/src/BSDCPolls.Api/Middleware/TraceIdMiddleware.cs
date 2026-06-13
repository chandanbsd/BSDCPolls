namespace BSDCPolls.Api.Middleware;

/// <summary>
/// Reads the <c>X-Trace-Id</c> header forwarded by the BFF and sets it as
/// <see cref="HttpContext.TraceIdentifier"/> so all log entries within the API
/// carry the same trace ID as the originating BFF request.
/// </summary>
public class TraceIdMiddleware
{
    private readonly RequestDelegate _next;

    /// <summary>Initialises a new instance of <see cref="TraceIdMiddleware"/>.</summary>
    public TraceIdMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    /// <summary>Invokes the middleware pipeline.</summary>
    public async Task InvokeAsync(HttpContext context)
    {
        if (
            context.Request.Headers.TryGetValue("X-Trace-Id", out var traceId)
            && !string.IsNullOrWhiteSpace(traceId)
        )
        {
            context.TraceIdentifier = traceId.ToString();
        }

        await _next(context);
    }
}
