using Cqrs.OrderService.Application.Abstractions.Messaging;
using Cqrs.OrderService.Application.Abstractions.Data;
using Cqrs.OrderService.Application.IntegrationEvents;

namespace Cqrs.OrderService.Application.IntegrationEventHandlers;

public sealed class OrderConfirmedIntegrationEventHandler(IIntegrationEventAuditRepository repository)
    : IIntegrationEventHandler<OrderConfirmedIntegrationEvent>
{
    public Task HandleAsync(OrderConfirmedIntegrationEvent integrationEvent, CancellationToken cancellationToken) =>
        repository.SaveAsync(
            new IntegrationEventAuditRecord(
                Guid.NewGuid().ToString(),
                integrationEvent.EventId,
                integrationEvent.EventType,
                "orders.confirmed",
                integrationEvent.OrderId,
                $"Confirmed order '{integrationEvent.OrderId}' for customer '{integrationEvent.CustomerId}'",
                integrationEvent.OccurredAt,
                DateTimeOffset.UtcNow),
            cancellationToken);
}
