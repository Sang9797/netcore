namespace Cqrs.OrderService.Application.IntegrationEvents;

public abstract record IntegrationEvent(
    string EventId,
    DateTimeOffset OccurredAt,
    string EventType);
