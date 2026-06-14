using System.Text.Json;
using BSDCPolls.Api.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;

namespace BSDCPolls.Api.Data.Infrastructure;

/// <summary>
/// SaveChanges interceptor that stamps audit fields (CreatedOn, CreatedById, UpdatedOn, UpdatedById)
/// on every <see cref="AuditableEntity"/> and writes a corresponding <see cref="AuditLog"/> entry.
/// Resolves <see cref="ICurrentUserContext"/> per-operation from the DI scope to support scoped lifetime.
/// </summary>
public sealed class AuditInterceptor : SaveChangesInterceptor
{
    private readonly IServiceProvider _serviceProvider;

    /// <summary>Initialises a new instance of <see cref="AuditInterceptor"/>.</summary>
    /// <param name="serviceProvider">Root service provider used to resolve scoped services per-operation.</param>
    public AuditInterceptor(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    /// <inheritdoc />
    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result
    )
    {
        StampAuditFields(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    /// <inheritdoc />
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default
    )
    {
        StampAuditFields(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void StampAuditFields(DbContext? context)
    {
        if (context is null)
        {
            return;
        }

        // Resolve ICurrentUserContext from the current scope (or fall back to sentinel)
        using var scope = _serviceProvider.CreateScope();
        var currentUser = scope.ServiceProvider.GetService<ICurrentUserContext>();
        var userId = currentUser?.UserId ?? 1; // 1 = system sentinel

        var now = DateTime.UtcNow;
        var auditLogs = new List<AuditLog>();

        foreach (var entry in context.ChangeTracker.Entries<AuditableEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    SetProperty(entry, nameof(AuditableEntity.CreatedOn), now);
                    SetProperty(entry, nameof(AuditableEntity.CreatedById), userId);
                    SetProperty(entry, nameof(AuditableEntity.UpdatedOn), now);
                    SetProperty(entry, nameof(AuditableEntity.UpdatedById), userId);
                    auditLogs.Add(BuildLog(entry, "INSERT", userId, now));
                    break;

                case EntityState.Modified:
                    SetProperty(entry, nameof(AuditableEntity.UpdatedOn), now);
                    SetProperty(entry, nameof(AuditableEntity.UpdatedById), userId);
                    auditLogs.Add(BuildLog(entry, "UPDATE", userId, now));
                    break;

                case EntityState.Deleted:
                    auditLogs.Add(BuildLog(entry, "DELETE", userId, now));
                    break;
            }
        }

        if (auditLogs.Count > 0)
        {
            context.Set<AuditLog>().AddRange(auditLogs);
        }
    }

    private static void SetProperty(
        Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<AuditableEntity> entry,
        string propertyName,
        object value
    )
    {
        entry.Property(propertyName).CurrentValue = value;
    }

    private static AuditLog BuildLog(
        Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<AuditableEntity> entry,
        string operation,
        int performedById,
        DateTime performedOn
    )
    {
        var entityId = (int)(entry.Property(nameof(AuditableEntity.Id)).CurrentValue ?? 0);
        var entityUid = (Guid)(
            entry.Property(nameof(AuditableEntity.Uid)).CurrentValue ?? Guid.Empty
        );

        // Serialize only scalar properties — ToObject() returns a lazy-loading proxy whose
        // navigation properties (CreatedBy, UpdatedBy, PerformedBy) would fire a SELECT each
        // when the JSON serializer walks every public property.
        var scalars = entry.Properties.ToDictionary(
            p => p.Metadata.Name,
            p => p.CurrentValue
        );
        var payload = JsonSerializer.Serialize(scalars, new JsonSerializerOptions { WriteIndented = false });

        // entry.Metadata.ClrType gives the real entity name, not the EF proxy subclass name.
        return AuditLog.Create(
            entityName: entry.Metadata.ClrType.Name,
            entityId: entityId,
            entityUid: entityUid,
            operation: operation,
            performedById: performedById,
            performedOn: performedOn,
            payload: payload
        );
    }
}
