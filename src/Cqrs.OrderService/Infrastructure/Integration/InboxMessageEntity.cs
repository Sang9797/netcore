namespace Cqrs.OrderService.Infrastructure.Integration;

public sealed class InboxMessageEntity
{
    public string InboxMessageId { get; set; } = default!;
    public string EventType { get; set; } = default!;
    public string RoutingKey { get; set; } = default!;
    public DateTimeOffset ReceivedAt { get; set; }
    public DateTimeOffset ProcessedAt { get; set; }
}
