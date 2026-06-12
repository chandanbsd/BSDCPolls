using BSDCPolls.Api.Data;
using BSDCPolls.Api.Data.Infrastructure;
using BSDCPolls.Api.Middleware;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.EntityFrameworkCore;
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

// ── FluentValidation ──────────────────────────────────────────────────────────
builder.Services
    .AddFluentValidationAutoValidation()
    .AddValidatorsFromAssemblyContaining<BSDCPolls.Contracts.AssemblyMarker>();

builder.Services.AddControllers();

// ── Application pipeline ──────────────────────────────────────────────────────
var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseMiddleware<TraceIdMiddleware>();

app.MapControllers();

app.Run();
