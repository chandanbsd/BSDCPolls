using BSDCPolls.Api.Data;
using BSDCPolls.Api.Data.Infrastructure;
using BSDCPolls.MigrationWorker;
using Microsoft.EntityFrameworkCore;

var builder = Host.CreateApplicationBuilder(args);

// The MigrationWorker connects directly to the database using the same connection
// string as the API. No HTTP context exists here, so CurrentUserContext falls back
// to the system sentinel (Id=1), which is correct for migration bootstrapping.
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserContext, CurrentUserContext>();
builder.Services.AddSingleton<AuditInterceptor>();

builder.Services.AddDbContext<BsdcPollsDbContext>(
    (sp, options) =>
    {
        var connectionString =
            builder.Configuration.GetConnectionString("BsdcPollsDb")
            ?? throw new InvalidOperationException(
                "Connection string 'BsdcPollsDb' is not configured."
            );

        options
            .UseNpgsql(
                connectionString,
                npgsql => npgsql.MigrationsAssembly(typeof(BsdcPollsDbContext).Assembly.FullName)
            )
            .UseLazyLoadingProxies()
            .AddInterceptors(sp.GetRequiredService<AuditInterceptor>());
    }
);

builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
