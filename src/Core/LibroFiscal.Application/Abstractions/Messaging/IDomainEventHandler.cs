using LibroFiscal.SharedKernel.Events;
using MediatR;

#pragma warning disable CA1711 // IDomainEventHandler — standard DDD naming, not a .NET event handler delegate

namespace LibroFiscal.Application.Abstractions.Messaging;

/// <summary>
/// Handler for domain events. Multiple handlers can subscribe to the same event.
/// Domain event handlers execute within the same transaction scope.
/// </summary>
/// <typeparam name="TEvent">The domain event type.</typeparam>
public interface IDomainEventHandler<in TEvent> : INotificationHandler<TEvent>
    where TEvent : IDomainEvent
{
}
