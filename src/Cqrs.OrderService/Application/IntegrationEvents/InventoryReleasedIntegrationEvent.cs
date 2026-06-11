namespace Cqrs.OrderService.Application.IntegrationEvents;

public sealed record InventoryReleasedIntegrationEvent(
    string EventId,
    DateTimeOffset OccurredAt,
    string ProductId,
    string WarehouseId,
    int Quantity,
    string OrderId)
    : IntegrationEvent(EventId, OccurredAt, nameof(InventoryReleasedIntegrationEvent));
