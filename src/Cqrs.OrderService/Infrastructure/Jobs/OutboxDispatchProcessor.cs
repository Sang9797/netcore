using Cqrs.OrderService.Infrastructure.Integration;
using Cqrs.OrderService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Cqrs.OrderService.Infrastructure.Jobs;

public sealed class OutboxDispatchProcessor(
    OrdersDbContext dbContext,
    RabbitMqMessagePublisher publisher,
    RabbitMqOptions options,
    ILogger<OutboxDispatchProcessor> logger)
{
    public async Task<int> DispatchBatchAsync(CancellationToken cancellationToken)
    {
        var messages = await dbContext.OutboxMessages
            .Where(message => message.PublishedAt == null)
            .OrderBy(message => message.CreatedAt)
            .Take(options.BatchSize)
            .ToListAsync(cancellationToken);

        foreach (var message in messages)
        {
            try
            {
                await publisher.PublishAsync(
                    message.OutboxMessageId,
                    message.EventType,
                    message.RoutingKey,
                    message.Payload,
                    cancellationToken);
                message.PublishedAt = DateTimeOffset.UtcNow;
                message.LastError = null;
            }
            catch (Exception exception)
            {
                message.LastError = exception.Message;
                logger.LogWarning(exception, "Failed to publish outbox message {OutboxMessageId}", message.OutboxMessageId);
            }

            message.PublishAttempts += 1;
        }

        if (messages.Count > 0)
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        return messages.Count;
    }
}
