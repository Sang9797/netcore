using Cqrs.OrderService.Application.Abstractions.Messaging;
using Cqrs.OrderService.Application.Abstractions.Data;
using Cqrs.OrderService.Application.IntegrationEvents;

namespace Cqrs.OrderService.Application.IntegrationEventHandlers;

public sealed class InventoryReleasedIntegrationEventHandler(IIntegrationEventAuditRepository repository)
    : IIntegrationEventHandler<InventoryReleasedIntegrationEvent>
{
    public Task HandleAsync(InventoryReleasedIntegrationEvent integrationEvent, CancellationToken cancellationToken) =>
        repository.SaveAsync(
            new IntegrationEventAuditRecord(
                Guid.NewGuid().ToString(),
                integrationEvent.EventId,
                integrationEvent.EventType,
                "inventory.released",
                $"{integrationEvent.ProductId}:{integrationEvent.WarehouseId}",
                $"Released {integrationEvent.Quantity} units for order '{integrationEvent.OrderId}'",
                integrationEvent.OccurredAt,
                DateTimeOffset.UtcNow),
            cancellationToken);
}
