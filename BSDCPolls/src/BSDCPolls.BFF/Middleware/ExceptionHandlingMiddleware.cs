using System.Net;
using System.Text.Json;
using BSDCPolls.Contracts.Responses;

namespace BSDCPolls.BFF.Middleware;

/// <summary>
/// Catches unhandled exceptions and writes a structured <see cref="ApiErrorResponse"/>
/// JSON body so the Angular client always receives a consistent error shape.
/// </summary>
public class ExceptionHandlingMiddleware
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    /// <summary>Initialises a new instance of <see cref="ExceptionHandlingMiddleware"/>.</summary>
    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    /// <summary>Invokes the middleware pipeline.</summary>
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception on {Method} {Path}", context.Request.Method, context.Request.Path);
            await WriteErrorAsync(context, ex);
        }
    }

    private static async Task WriteErrorAsync(HttpContext context, Exception ex)
    {
        var traceId = context.TraceIdentifier;

        var (statusCode, message) = ex switch
        {
            UnauthorizedAccessException => (HttpStatusCode.Forbidden, "Access denied."),
            KeyNotFoundException => (HttpStatusCode.NotFound, "Resource not found."),
            ArgumentException ae => (HttpStatusCode.BadRequest, ae.Message),
            InvalidOperationException ioe => (HttpStatusCode.UnprocessableEntity, ioe.Message),
            _ => (HttpStatusCode.InternalServerError, "An unexpected error occurred."),
        };

        context.Response.StatusCode = (int)statusCode;
        context.Response.ContentType = "application/json";

        var body = new ApiErrorResponse(traceId, (int)statusCode, message, null);
        await context.Response.WriteAsync(JsonSerializer.Serialize(body, SerializerOptions));
    }
}
