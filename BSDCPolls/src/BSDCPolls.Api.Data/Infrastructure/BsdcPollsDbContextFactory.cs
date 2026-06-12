using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.DependencyInjection;

namespace BSDCPolls.Api.Data.Infrastructure;

/// <summary>
/// Design-time factory for <see cref="BsdcPollsDbContext"/>, used by EF Core CLI tools
/// (migrations, scaffolding) when no running host is available.
/// Connects to a local PostgreSQL instance via environment variable or a hardcoded dev default.
/// </summary>
public sealed class BsdcPollsDbContextFactory : IDesignTimeDbContextFactory<BsdcPollsDbContext>
{
    /// <inheritdoc />
    public BsdcPollsDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("BSDCPOLLS_DB")
            ?? "Host=localhost;Port=5432;Database=bsdcpolls;Username=postgres;Password=postgres";

        var services = new ServiceCollection();
        services.AddSingleton<ICurrentUserContext, DesignTimeUserContext>();
        services.AddSingleton<AuditInterceptor>();

        var sp = services.BuildServiceProvider();

        var optionsBuilder = new DbContextOptionsBuilder<BsdcPollsDbContext>();
        optionsBuilder
            .UseNpgsql(connectionString)
            .UseLazyLoadingProxies()
            .AddInterceptors(sp.GetRequiredService<AuditInterceptor>());

        return new BsdcPollsDbContext(optionsBuilder.Options);
    }

    /// <summary>Stub user context for design-time operations — always returns system sentinel.</summary>
    private sealed class DesignTimeUserContext : ICurrentUserContext
    {
        public int UserId => 1;
    }
}
