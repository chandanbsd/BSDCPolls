using System.Text;
using BSDCPolls.Api.Business.Auth;
using BSDCPolls.Api.Business.Invitations;
using BSDCPolls.Api.Business.Notifications;
using BSDCPolls.Api.Business.Polls;
using BSDCPolls.Api.Business.Surveys;
using BSDCPolls.Api.Business.UsernameGeneration;
using BSDCPolls.Api.Data;
using BSDCPolls.Api.Data.Infrastructure;
using BSDCPolls.Api.Data.Repositories;
using BSDCPolls.Api.Middleware;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
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
    .WriteTo.File("logs/api-.log", rollingInterval: RollingInterval.Day));

// ── OpenTelemetry ─────────────────────────────────────────────────────────────
var otlpEndpoint = builder.Configuration["Otlp:Endpoint"] ?? "http://localhost:4317";

builder.Services
    .AddOpenTelemetry()
    .ConfigureResource(r => r.AddService("BSDCPolls.Api"))
    .WithTracing(t => t
        .AddAspNetCoreInstrumentation()
        .AddEntityFrameworkCoreInstrumentation()
        .AddOtlpExporter(o => o.Endpoint = new Uri(otlpEndpoint)))
    .WithMetrics(m => m
        .AddAspNetCoreInstrumentation()
        .AddOtlpExporter(o => o.Endpoint = new Uri(otlpEndpoint)));

// ── EF Core + Audit infrastructure ───────────────────────────────────────────
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserContext, CurrentUserContext>();
builder.Services.AddSingleton<AuditInterceptor>();

builder.Services.AddDbContext<BsdcPollsDbContext>((sp, options) =>
{
    var connectionString = builder.Configuration.GetConnectionString("BsdcPollsDb")
        ?? throw new InvalidOperationException("Connection string 'BsdcPollsDb' is not configured.");

    options
        .UseNpgsql(connectionString)
        .UseLazyLoadingProxies()
        .AddInterceptors(sp.GetRequiredService<AuditInterceptor>());
});

// ── JWT Bearer (GoTrue JWTs validated by the API for [Authorize] endpoints) ───
var jwtSecret = builder.Configuration["GoTrue__JwtSecret"]
    ?? "super-secret-jwt-token-for-dev-only";

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret))
        };
    });

builder.Services.AddAuthorization();

// ── Repositories ─────────────────────────────────────────────────────────────
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUsernameHistoryRepository, UsernameHistoryRepository>();
builder.Services.AddScoped<IPollRepository, PollRepository>();
builder.Services.AddScoped<IPollSubmissionRepository, PollSubmissionRepository>();
builder.Services.AddScoped<ISurveyRepository, SurveyRepository>();
builder.Services.AddScoped<ISurveyResponseRepository, SurveyResponseRepository>();
builder.Services.AddScoped<ISurveyDocumentRepository, SurveyDocumentRepository>();
builder.Services.AddScoped<IInvitationRepository, InvitationRepository>();
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();

// ── Business services ─────────────────────────────────────────────────────────
builder.Services.AddSingleton<IUsernameGenerator, UsernameGeneratorService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IPollService, PollService>();
builder.Services.AddScoped<ISurveyService, SurveyService>();
builder.Services.AddScoped<IInvitationService, InvitationService>();
builder.Services.AddScoped<INotificationService, NotificationService>();

// ── GoTrue HttpClient ─────────────────────────────────────────────────────────
var goTrueUrl = builder.Configuration["GoTrue__Url"]
    ?? "http://localhost:9999";

builder.Services.AddHttpClient("GoTrue", client =>
{
    client.BaseAddress = new Uri(goTrueUrl);
});

// ── BFF internal HttpClient (for SignalR notification push) ───────────────────
var bffInternalUrl = builder.Configuration["Bff__InternalUrl"]
    ?? "http://localhost:5000";

builder.Services.AddHttpClient("BffInternal", client =>
{
    client.BaseAddress = new Uri(bffInternalUrl);
});

// ── FluentValidation ──────────────────────────────────────────────────────────
builder.Services
    .AddFluentValidationAutoValidation()
    .AddValidatorsFromAssemblyContaining<BSDCPolls.Contracts.AssemblyMarker>();

builder.Services.AddControllers();

// ── Application pipeline ──────────────────────────────────────────────────────
var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseMiddleware<TraceIdMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
