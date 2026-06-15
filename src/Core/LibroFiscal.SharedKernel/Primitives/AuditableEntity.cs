namespace LibroFiscal.SharedKernel.Primitives;

/// <summary>
/// Base class for auditable entities. Tracks creation and modification metadata.
/// All entities that need audit trail must inherit from this.
/// The infrastructure layer (EF Core SaveChanges interceptor) populates these fields automatically.
/// </summary>
/// <typeparam name="TId">The type of the entity identifier.</typeparam>
public abstract class AuditableEntity<TId> : Entity<TId>, IAuditableEntity
    where TId : notnull
{
    /// <summary>UTC timestamp when the entity was first created.</summary>
    public DateTimeOffset CreatedAt { get; private set; }

    /// <summary>Identifier of the user/system that created the entity.</summary>
    public string CreatedBy { get; private set; } = string.Empty;

    /// <summary>UTC timestamp of the last modification. Null if never modified after creation.</summary>
    public DateTimeOffset? LastModifiedAt { get; private set; }

    /// <summary>Identifier of the user/system that last modified the entity.</summary>
    public string? LastModifiedBy { get; private set; }

    protected AuditableEntity() { }

    protected AuditableEntity(TId id) : base(id) { }

    /// <summary>
    /// Sets creation audit metadata. Called by infrastructure on initial persist.
    /// </summary>
    public void SetCreatedAudit(DateTimeOffset timestamp, string userId)
    {
        CreatedAt = timestamp;
        CreatedBy = userId;
    }

    /// <summary>
    /// Sets modification audit metadata. Called by infrastructure on update.
    /// </summary>
    public void SetModifiedAudit(DateTimeOffset timestamp, string userId)
    {
        LastModifiedAt = timestamp;
        LastModifiedBy = userId;
    }
}

/// <summary>
/// Auditable aggregate root combining aggregate behavior with audit trail.
/// </summary>
/// <typeparam name="TId">The type of the aggregate root identifier.</typeparam>
public abstract class AuditableAggregateRoot<TId> : AggregateRoot<TId>, IAuditableEntity
    where TId : notnull
{
    public DateTimeOffset CreatedAt { get; private set; }
    public string CreatedBy { get; private set; } = string.Empty;
    public DateTimeOffset? LastModifiedAt { get; private set; }
    public string? LastModifiedBy { get; private set; }

    protected AuditableAggregateRoot() { }

    protected AuditableAggregateRoot(TId id) : base(id) { }

    public void SetCreatedAudit(DateTimeOffset timestamp, string userId)
    {
        CreatedAt = timestamp;
        CreatedBy = userId;
    }

    public void SetModifiedAudit(DateTimeOffset timestamp, string userId)
    {
        LastModifiedAt = timestamp;
        LastModifiedBy = userId;
    }
}
