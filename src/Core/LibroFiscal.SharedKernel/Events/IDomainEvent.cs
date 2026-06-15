using MediatR;

namespace LibroFiscal.SharedKernel.Events;

/// <summary>
/// Marker interface for domain events.
/// Domain events represent something meaningful that happened within a bounded context.
/// They are dispatched after the aggregate is persisted (post-commit) via MediatR.
/// 
/// Domain events are internal to the bounded context — for cross-context communication,
/// use <see cref="IIntegrationEvent"/>.
/// 
/// Examples:
/// - DteCreatedEvent (DTE context)
/// - DteSealedEvent (DTE context → triggers FiscalBooks update)
/// - CompanyConfiguredEvent (Companies context)
/// </summary>
public interface IDomainEvent : INotification
{
    /// <summary>
    /// Unique identifier for this event instance.
    /// </summary>
    Guid EventId { get; }

    /// <summary>
    /// UTC timestamp when the event occurred.
    /// </summary>
    DateTimeOffset OccurredAt { get; }
}

/// <summary>
/// Base record for domain events with automatic ID and timestamp.
/// </summary>
public abstract record DomainEvent : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow;
}
