namespace BSDCPolls.BFF.Middleware;

/// <summary>
/// Propagates the W3C <c>traceparent</c> header from inbound requests into the
/// <see cref="HttpContext.TraceIdentifier"/> so that trace IDs are consistent
/// across the Angular → BFF → API chain.
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
            context.Request.Headers.TryGetValue("traceparent", out var traceparent)
            && !string.IsNullOrWhiteSpace(traceparent)
        )
        {
            context.TraceIdentifier = traceparent.ToString();
        }

        context.Response.Headers["X-Trace-Id"] = context.TraceIdentifier;

        await _next(context);
    }
}
