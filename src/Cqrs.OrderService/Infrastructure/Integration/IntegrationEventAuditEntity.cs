namespace Cqrs.OrderService.Infrastructure.Integration;

public sealed class IntegrationEventAuditEntity
{
    public string AuditId { get; set; } = default!;
    public string MessageId { get; set; } = default!;
    public string EventType { get; set; } = default!;
    public string RoutingKey { get; set; } = default!;
    public string AggregateId { get; set; } = default!;
    public string Description { get; set; } = default!;
    public DateTimeOffset OccurredAt { get; set; }
    public DateTimeOffset ProcessedAt { get; set; }
}
