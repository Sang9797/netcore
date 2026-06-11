using Cqrs.OrderService.Application.IntegrationEvents;

namespace Cqrs.OrderService.Application.Abstractions.Messaging;

public interface IIntegrationEventHandler<in TIntegrationEvent>
    where TIntegrationEvent : IntegrationEvent
{
    Task HandleAsync(TIntegrationEvent integrationEvent, CancellationToken cancellationToken);
}
