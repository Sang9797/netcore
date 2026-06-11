using Cqrs.OrderService.Application.Abstractions.Messaging;
using Cqrs.OrderService.Application.Abstractions.Data;
using Cqrs.OrderService.Application.IntegrationEvents;

namespace Cqrs.OrderService.Application.IntegrationEventHandlers;

public sealed class OrderPlacedIntegrationEventHandler(IIntegrationEventAuditRepository repository)
    : IIntegrationEventHandler<OrderPlacedIntegrationEvent>
{
    public Task HandleAsync(OrderPlacedIntegrationEvent integrationEvent, CancellationToken cancellationToken) =>
        repository.SaveAsync(
            new IntegrationEventAuditRecord(
                Guid.NewGuid().ToString(),
                integrationEvent.EventId,
                integrationEvent.EventType,
                "orders.placed",
                integrationEvent.OrderId,
                $"Placed order '{integrationEvent.OrderId}' for customer '{integrationEvent.CustomerId}' with {integrationEvent.Items.Count} items",
                integrationEvent.OccurredAt,
                DateTimeOffset.UtcNow),
            cancellationToken);
}
