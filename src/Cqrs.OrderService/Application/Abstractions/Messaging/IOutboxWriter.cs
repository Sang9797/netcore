using Cqrs.OrderService.Application.IntegrationEvents;

namespace Cqrs.OrderService.Application.Abstractions.Messaging;

public interface IOutboxWriter
{
    Task EnqueueAsync(IntegrationEvent integrationEvent, CancellationToken cancellationToken);
}
