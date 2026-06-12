using System.ComponentModel.DataAnnotations;

namespace BSDCPolls.Api.Data.Entities;

/// <summary>
/// Immutable record of every INSERT, UPDATE, or soft-DELETE applied to any
/// <see cref="AuditableEntity"/> subclass. Written by <c>AuditInterceptor</c>;
/// never modified after creation.
/// </summary>
public class AuditLog
{
    /// <summary>Auto-increment primary key.</summary>
    public long Id { get; private set; }

    /// <summary>Name of the entity type that changed (e.g., <c>ApplicationUser</c>).</summary>
    [Required]
    [MaxLength(100)]
    public string EntityName { get; private set; } = string.Empty;

    /// <summary>Internal integer PK of the entity that changed.</summary>
    public int EntityId { get; private set; }

    /// <summary>Public GUID of the entity that changed.</summary>
    public Guid EntityUid { get; private set; }

    /// <summary>Operation that triggered this log entry: INSERT, UPDATE, or DELETE.</summary>
    [Required]
    [MaxLength(10)]
    public string Operation { get; private set; } = string.Empty;

    /// <summary>FK to the ApplicationUser who performed the operation.</summary>
    public int PerformedById { get; private set; }

    /// <summary>Navigation property to the user who performed the operation.</summary>
    public virtual ApplicationUser PerformedBy { get; private set; } = null!;

    /// <summary>UTC timestamp of the operation.</summary>
    public DateTime PerformedOn { get; private set; }

    /// <summary>
    /// JSON snapshot of the entity state after the operation.
    /// Stored as JSONB for queryability.
    /// </summary>
    public string? Payload { get; private set; }

    /// <summary>EF Core proxy constructor.</summary>
    protected AuditLog()
    {
    }

    private AuditLog(
        string entityName,
        int entityId,
        Guid entityUid,
        string operation,
        int performedById,
        DateTime performedOn,
        string? payload)
    {
        EntityName = entityName;
        EntityId = entityId;
        EntityUid = entityUid;
        Operation = operation;
        PerformedById = performedById;
        PerformedOn = performedOn;
        Payload = payload;
    }

    /// <summary>Creates an audit log entry for an entity state change.</summary>
    /// <param name="entityName">The CLR type name of the changed entity.</param>
    /// <param name="entityId">The integer primary key of the changed entity.</param>
    /// <param name="entityUid">The GUID public identifier of the changed entity.</param>
    /// <param name="operation">INSERT, UPDATE, or DELETE.</param>
    /// <param name="performedById">FK to the ApplicationUser who caused the change.</param>
    /// <param name="performedOn">UTC time of the operation.</param>
    /// <param name="payload">Optional JSON snapshot of entity state after the change.</param>
    /// <returns>A new <see cref="AuditLog"/> entry ready for persistence.</returns>
    public static AuditLog Create(
        string entityName,
        int entityId,
        Guid entityUid,
        string operation,
        int performedById,
        DateTime performedOn,
        string? payload = null)
    {
        return new AuditLog(entityName, entityId, entityUid, operation, performedById, performedOn, payload);
    }
}
