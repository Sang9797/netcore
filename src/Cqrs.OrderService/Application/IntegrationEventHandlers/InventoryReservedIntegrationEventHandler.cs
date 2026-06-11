using Cqrs.OrderService.Application.Abstractions.Messaging;
using Cqrs.OrderService.Application.Abstractions.Data;
using Cqrs.OrderService.Application.IntegrationEvents;

namespace Cqrs.OrderService.Application.IntegrationEventHandlers;

public sealed class InventoryReservedIntegrationEventHandler(IIntegrationEventAuditRepository repository)
    : IIntegrationEventHandler<InventoryReservedIntegrationEvent>
{
    public Task HandleAsync(InventoryReservedIntegrationEvent integrationEvent, CancellationToken cancellationToken) =>
        repository.SaveAsync(
            new IntegrationEventAuditRecord(
                Guid.NewGuid().ToString(),
                integrationEvent.EventId,
                integrationEvent.EventType,
                "inventory.reserved",
                $"{integrationEvent.ProductId}:{integrationEvent.WarehouseId}",
                $"Reserved {integrationEvent.Quantity} units for order '{integrationEvent.OrderId}'",
                integrationEvent.OccurredAt,
                DateTimeOffset.UtcNow),
            cancellationToken);
}
