namespace BSDCPolls.Api.Data.Entities;

/// <summary>
/// Base class for all EF Core entities. Enforces the dual-key pattern (int Id + Guid Uid),
/// soft-delete via IsActive, and full audit trail via the SaveChangesInterceptor.
/// All subclasses MUST use private setters and static factory methods.
/// </summary>
public abstract class AuditableEntity
{
    /// <summary>Auto-increment integer primary key. Used internally and for FK references only; never exposed to the frontend.</summary>
    public int Id { get; private set; }

    /// <summary>Publicly exposed GUID identifier, set once at creation. The only ID surfaced to the frontend and external APIs.</summary>
    public Guid Uid { get; private set; }

    /// <summary>Soft-delete flag. Defaults to true. Hard deletes are prohibited; use Deactivate methods instead.</summary>
    public bool IsActive { get; private set; }

    /// <summary>UTC timestamp set by <see cref="Infrastructure.AuditInterceptor"/> on INSERT. Never set manually in service code.</summary>
    public DateTime CreatedOn { get; private set; }

    /// <summary>FK to the ApplicationUser who created this entity. Set once on creation by the audit interceptor.</summary>
    public int CreatedById { get; private set; }

    /// <summary>Navigation property to the creating user.</summary>
    public virtual ApplicationUser CreatedBy { get; private set; } = null!;

    /// <summary>UTC timestamp set by <see cref="Infrastructure.AuditInterceptor"/> on every UPDATE.</summary>
    public DateTime UpdatedOn { get; private set; }

    /// <summary>FK to the ApplicationUser who last updated this entity. Updated by the audit interceptor on every change.</summary>
    public int UpdatedById { get; private set; }

    /// <summary>Navigation property to the last updating user.</summary>
    public virtual ApplicationUser UpdatedBy { get; private set; } = null!;

    /// <summary>
    /// Protected constructor for EF Core proxy support.
    /// Subclasses must also provide a private full constructor called only by their static factory method.
    /// </summary>
    protected AuditableEntity() { }

    /// <summary>
    /// Initialises the immutable identity fields. Called only from subclass factory methods.
    /// </summary>
    /// <param name="uid">Pre-generated GUID for the public identifier.</param>
    protected void InitialiseIdentity(Guid uid)
    {
        Uid = uid;
        IsActive = true;
    }

    /// <summary>Marks the entity as inactive (soft delete). Sets <see cref="IsActive"/> to false.</summary>
    protected void MarkInactive()
    {
        IsActive = false;
    }
}
