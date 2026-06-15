using LibroFiscal.SharedKernel.Events;

namespace LibroFiscal.SharedKernel.Primitives;

/// <summary>
/// Base class for aggregate roots. Aggregates are the transactional consistency boundary.
/// Only aggregate roots can be directly referenced by other aggregates (via their ID).
/// Domain events are raised by aggregates and dispatched after persistence.
/// </summary>
/// <typeparam name="TId">The type of the aggregate root identifier.</typeparam>
public abstract class AggregateRoot<TId> : Entity<TId>
    where TId : notnull
{
    private readonly List<IDomainEvent> _domainEvents = [];

    /// <summary>
    /// Domain events raised by this aggregate, pending dispatch.
    /// </summary>
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    protected AggregateRoot() { }

    protected AggregateRoot(TId id) : base(id) { }

    /// <summary>
    /// Raises a domain event to be dispatched after the aggregate is persisted.
    /// </summary>
    protected void RaiseDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    /// <summary>
    /// Clears all pending domain events. Called by the infrastructure after dispatch.
    /// </summary>
    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
}
