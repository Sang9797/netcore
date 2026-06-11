using Cqrs.OrderService.Application.Abstractions.Messaging;
using Cqrs.OrderService.Application.Abstractions.Data;
using Cqrs.OrderService.Application.IntegrationEvents;

namespace Cqrs.OrderService.Application.IntegrationEventHandlers;

public sealed class InventoryAdjustedIntegrationEventHandler(IIntegrationEventAuditRepository repository)
    : IIntegrationEventHandler<InventoryAdjustedIntegrationEvent>
{
    public Task HandleAsync(InventoryAdjustedIntegrationEvent integrationEvent, CancellationToken cancellationToken) =>
        repository.SaveAsync(
            new IntegrationEventAuditRecord(
                Guid.NewGuid().ToString(),
                integrationEvent.EventId,
                integrationEvent.EventType,
                "inventory.adjusted",
                $"{integrationEvent.ProductId}:{integrationEvent.WarehouseId}",
                $"Adjusted inventory by {integrationEvent.Delta} for product '{integrationEvent.ProductId}' in warehouse '{integrationEvent.WarehouseId}'",
                integrationEvent.OccurredAt,
                DateTimeOffset.UtcNow),
            cancellationToken);
}
