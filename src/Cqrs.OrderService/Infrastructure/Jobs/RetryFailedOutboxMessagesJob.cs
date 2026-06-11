using Cqrs.OrderService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Cqrs.OrderService.Infrastructure.Jobs;

public sealed class RetryFailedOutboxMessagesJob(
    OrdersDbContext dbContext,
    OutboxDispatchProcessor processor,
    HangfireOptions options,
    ILogger<RetryFailedOutboxMessagesJob> logger)
{
    public async Task RunAsync(CancellationToken cancellationToken)
    {
        var failedBefore = await dbContext.OutboxMessages
            .AsNoTracking()
            .CountAsync(
                message => message.PublishedAt == null
                    && message.PublishAttempts > 0
                    && message.LastError != null,
                cancellationToken);

        if (failedBefore == 0)
        {
            logger.LogInformation("Retry-failed-outbox job found no failed messages to retry");
            return;
        }

        var processedTotal = 0;
        for (var batch = 0; batch < options.RetryFailedOutboxMaxBatches; batch += 1)
        {
            var processed = await processor.DispatchBatchAsync(cancellationToken);
            processedTotal += processed;

            if (processed == 0)
            {
                break;
            }
        }

        var failedAfter = await dbContext.OutboxMessages
            .AsNoTracking()
            .CountAsync(
                message => message.PublishedAt == null
                    && message.PublishAttempts > 0
                    && message.LastError != null,
                cancellationToken);

        logger.LogInformation(
            "Retry-failed-outbox job started with {FailedBefore} failed messages, processed {ProcessedTotal} messages, and ended with {FailedAfter} failed messages",
            failedBefore,
            processedTotal,
            failedAfter);
    }
}
