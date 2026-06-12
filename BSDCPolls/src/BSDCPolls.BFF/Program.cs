using System.Text;
using BSDCPolls.BFF.Business.Auth;
using BSDCPolls.BFF.Middleware;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// ── Serilog ───────────────────────────────────────────────────────────────────
builder.Host.UseSerilog((ctx, lc) => lc
    .ReadFrom.Configuration(ctx.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/bff-.log", rollingInterval: RollingInterval.Day));

// ── OpenTelemetry ─────────────────────────────────────────────────────────────
var otlpEndpoint = builder.Configuration["Otlp:Endpoint"] ?? "http://localhost:4317";

builder.Services
    .AddOpenTelemetry()
    .ConfigureResource(r => r.AddService("BSDCPolls.BFF"))
    .WithTracing(t => t
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddOtlpExporter(o => o.Endpoint = new Uri(otlpEndpoint)))
    .WithMetrics(m => m
        .AddAspNetCoreInstrumentation()
        .AddOtlpExporter(o => o.Endpoint = new Uri(otlpEndpoint)));

// ── Authentication (Supabase GoTrue JWT — symmetric key validation) ───────────
var jwtSecret = builder.Configuration["GoTrue__JwtSecret"]
    ?? "super-secret-jwt-token-for-dev-only";

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
            ClockSkew = TimeSpan.FromSeconds(30),
        };

        // Allow JWT in the SignalR query string (WebSocket cannot send headers)
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = ctx =>
            {
                var accessToken = ctx.Request.Query["access_token"];
                var path = ctx.HttpContext.Request.Path;
                if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
                {
                    ctx.Token = accessToken;
                }

                return Task.CompletedTask;
            },
        };
    });

builder.Services.AddAuthorization();

// ── BFF auth service ──────────────────────────────────────────────────────────
builder.Services.AddScoped<IBffAuthService, BffAuthService>();

// ── SignalR ────────────────────────────────────────────────────────────────────
builder.Services.AddSignalR();

// ── CORS ──────────────────────────────────────────────────────────────────────
var angularOrigin = builder.Configuration["AllowedOrigins:Angular"] ?? "http://localhost:4200";
builder.Services.AddCors(options =>
{
    options.AddPolicy("AngularPolicy", policy =>
    {
        policy
            .WithOrigins(angularOrigin)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials(); // required for SignalR WebSocket
    });
});

// ── HTTP client (BFF → internal API forwarding) ───────────────────────────────
var internalApiUrl = builder.Configuration["InternalApi:Url"]
    ?? throw new InvalidOperationException("InternalApi:Url configuration is required.");

builder.Services.AddHttpClient("InternalApi", client =>
{
    client.BaseAddress = new Uri(internalApiUrl);
});

// ── FluentValidation ──────────────────────────────────────────────────────────
builder.Services
    .AddFluentValidationAutoValidation()
    .AddValidatorsFromAssemblyContaining<BSDCPolls.Contracts.AssemblyMarker>();

// ── NSwag / OpenAPI ───────────────────────────────────────────────────────────
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApiDocument(config =>
{
    config.Title = "BSDCPolls BFF";
    config.Version = "v1";
});

builder.Services.AddControllers();

// ── Application pipeline ──────────────────────────────────────────────────────
var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseMiddleware<TraceIdMiddleware>();

app.UseCors("AngularPolicy");

if (app.Environment.IsDevelopment())
{
    app.UseOpenApi();
    app.UseSwaggerUi();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// SignalR hubs — mapped when hub classes are added in Phase 4 / Phase 6
// app.MapHub<PollHub>("/hubs/poll");
// app.MapHub<NotificationHub>("/hubs/notifications");

app.Run();
