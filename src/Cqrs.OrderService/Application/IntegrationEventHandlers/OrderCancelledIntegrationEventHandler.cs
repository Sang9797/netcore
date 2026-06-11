using Cqrs.OrderService.Application.Abstractions.Messaging;
using Cqrs.OrderService.Application.Abstractions.Data;
using Cqrs.OrderService.Application.IntegrationEvents;

namespace Cqrs.OrderService.Application.IntegrationEventHandlers;

public sealed class OrderCancelledIntegrationEventHandler(IIntegrationEventAuditRepository repository)
    : IIntegrationEventHandler<OrderCancelledIntegrationEvent>
{
    public Task HandleAsync(OrderCancelledIntegrationEvent integrationEvent, CancellationToken cancellationToken) =>
        repository.SaveAsync(
            new IntegrationEventAuditRecord(
                Guid.NewGuid().ToString(),
                integrationEvent.EventId,
                integrationEvent.EventType,
                "orders.cancelled",
                integrationEvent.OrderId,
                $"Cancelled order '{integrationEvent.OrderId}' for customer '{integrationEvent.CustomerId}'",
                integrationEvent.OccurredAt,
                DateTimeOffset.UtcNow),
            cancellationToken);
}
