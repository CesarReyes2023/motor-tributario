namespace LibroFiscal.SharedKernel.Events;

/// <summary>
/// Marker interface for integration events.
/// Integration events cross bounded context boundaries and are used for
/// inter-module communication within the modular monolith.
/// 
/// In Phase 1, these are dispatched in-process via MediatR.
/// In future SaaS phases, they will be published to RabbitMQ/Azure Service Bus.
/// 
/// Examples:
/// - DteSealedIntegrationEvent (DTE → FiscalBooks, Audit, Reporting)
/// - CompanyCreatedIntegrationEvent (Companies → DTE, FiscalEngine)
/// </summary>
public interface IIntegrationEvent
{
    /// <summary>
    /// Unique identifier for this event instance.
    /// </summary>
    Guid EventId { get; }

    /// <summary>
    /// UTC timestamp when the event was published.
    /// </summary>
    DateTimeOffset OccurredAt { get; }

    /// <summary>
    /// Correlation ID linking this event to the originating operation.
    /// Enables distributed tracing across module boundaries.
    /// </summary>
    string? CorrelationId { get; }
}

/// <summary>
/// Base record for integration events.
/// </summary>
public abstract record IntegrationEvent : IIntegrationEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow;
    public string? CorrelationId { get; init; }
}
