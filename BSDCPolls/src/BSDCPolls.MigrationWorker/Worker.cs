using BSDCPolls.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace BSDCPolls.MigrationWorker;

/// <summary>
/// One-shot hosted service that applies all pending EF Core migrations on startup
/// then signals the host to shut down gracefully.
/// </summary>
public class Worker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IHostApplicationLifetime _lifetime;
    private readonly ILogger<Worker> _logger;

    /// <summary>Initialises a new instance of <see cref="Worker"/>.</summary>
    /// <param name="scopeFactory">Factory for creating DI scopes.</param>
    /// <param name="lifetime">Host lifetime for triggering shutdown after work completes.</param>
    /// <param name="logger">Logger.</param>
    public Worker(
        IServiceScopeFactory scopeFactory,
        IHostApplicationLifetime lifetime,
        ILogger<Worker> logger)
    {
        _scopeFactory = scopeFactory;
        _lifetime = lifetime;
        _logger = logger;
    }

    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            _logger.LogInformation("Migration worker starting.");

            await using var scope = _scopeFactory.CreateAsyncScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<BsdcPollsDbContext>();

            _logger.LogInformation("Applying pending migrations…");
            await dbContext.Database.MigrateAsync(stoppingToken);
            _logger.LogInformation("Migrations applied successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "Migration worker encountered a fatal error.");
            _lifetime.StopApplication();
            throw;
        }

        _lifetime.StopApplication();
    }
}
