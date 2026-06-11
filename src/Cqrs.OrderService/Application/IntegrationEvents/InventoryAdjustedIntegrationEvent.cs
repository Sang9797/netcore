namespace Cqrs.OrderService.Application.IntegrationEvents;

public sealed record InventoryAdjustedIntegrationEvent(
    string EventId,
    DateTimeOffset OccurredAt,
    string ProductId,
    string WarehouseId,
    int Delta,
    string Reason)
    : IntegrationEvent(EventId, OccurredAt, nameof(InventoryAdjustedIntegrationEvent));
