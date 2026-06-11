namespace Cqrs.OrderService.Application.IntegrationEvents;

public sealed record OrderConfirmedIntegrationEvent(
    string EventId,
    DateTimeOffset OccurredAt,
    string OrderId,
    string CustomerId)
    : IntegrationEvent(EventId, OccurredAt, nameof(OrderConfirmedIntegrationEvent));
