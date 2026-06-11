using System.Text.Json;
using Cqrs.OrderService.Application.Abstractions.Messaging;
using Cqrs.OrderService.Application.IntegrationEvents;
using Cqrs.OrderService.Infrastructure.Persistence;

namespace Cqrs.OrderService.Infrastructure.Integration;

public sealed class OutboxWriter(OrdersDbContext dbContext) : IOutboxWriter
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    public async Task EnqueueAsync(IntegrationEvent integrationEvent, CancellationToken cancellationToken)
    {
        dbContext.OutboxMessages.Add(new OutboxMessageEntity
        {
            OutboxMessageId = integrationEvent.EventId,
            EventType = integrationEvent.EventType,
            RoutingKey = RoutingKeyFor(integrationEvent.EventType),
            Payload = JsonSerializer.Serialize(integrationEvent, integrationEvent.GetType(), SerializerOptions),
            OccurredAt = integrationEvent.OccurredAt,
            CreatedAt = DateTimeOffset.UtcNow
        });

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static string RoutingKeyFor(string eventType) =>
        eventType switch
        {
            nameof(OrderPlacedIntegrationEvent) => "orders.placed",
            nameof(OrderConfirmedIntegrationEvent) => "orders.confirmed",
            nameof(OrderCancelledIntegrationEvent) => "orders.cancelled",
            nameof(InventoryReservedIntegrationEvent) => "inventory.reserved",
            nameof(InventoryReleasedIntegrationEvent) => "inventory.released",
            nameof(InventoryAdjustedIntegrationEvent) => "inventory.adjusted",
            _ => "events.unknown"
        };
}
