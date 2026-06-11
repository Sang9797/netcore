namespace Cqrs.OrderService.Infrastructure.Integration;

public sealed class OutboxMessageEntity
{
    public string OutboxMessageId { get; set; } = default!;
    public string EventType { get; set; } = default!;
    public string RoutingKey { get; set; } = default!;
    public string Payload { get; set; } = default!;
    public DateTimeOffset OccurredAt { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? PublishedAt { get; set; }
    public int PublishAttempts { get; set; }
    public string? LastError { get; set; }
}
