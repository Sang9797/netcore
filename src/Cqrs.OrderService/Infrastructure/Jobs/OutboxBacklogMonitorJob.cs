using Cqrs.OrderService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Cqrs.OrderService.Infrastructure.Jobs;

public sealed class OutboxBacklogMonitorJob(
    OrdersDbContext dbContext,
    ILogger<OutboxBacklogMonitorJob> logger)
{
    public async Task CheckBacklogAsync(CancellationToken cancellationToken)
    {
        var pendingCount = await dbContext.OutboxMessages
            .CountAsync(message => message.PublishedAt == null, cancellationToken);

        var failedCount = await dbContext.OutboxMessages
            .CountAsync(
                message => message.PublishedAt == null
                    && message.PublishAttempts > 0
                    && message.LastError != null,
                cancellationToken);

        if (pendingCount == 0)
        {
            logger.LogInformation("Outbox backlog monitor found no pending messages");
            return;
        }

        logger.LogWarning(
            "Outbox backlog monitor found {PendingCount} pending messages and {FailedCount} failed messages",
            pendingCount,
            failedCount);
    }
}
