namespace Cqrs.OrderService.Application.IntegrationEvents;

public sealed record OrderCancelledIntegrationEvent(
    string EventId,
    DateTimeOffset OccurredAt,
    string OrderId,
    string CustomerId,
    string Reason)
    : IntegrationEvent(EventId, OccurredAt, nameof(OrderCancelledIntegrationEvent));
