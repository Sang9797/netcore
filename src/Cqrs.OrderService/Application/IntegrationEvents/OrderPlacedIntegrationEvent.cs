namespace Cqrs.OrderService.Application.IntegrationEvents;

public sealed record OrderPlacedIntegrationEvent(
    string EventId,
    DateTimeOffset OccurredAt,
    string OrderId,
    string CustomerId,
    decimal TotalAmount,
    string Currency,
    IReadOnlyList<OrderPlacedItem> Items)
    : IntegrationEvent(EventId, OccurredAt, nameof(OrderPlacedIntegrationEvent));

public sealed record OrderPlacedItem(
    string ProductId,
    string ProductName,
    int Quantity,
    decimal UnitPrice,
    string Currency);
