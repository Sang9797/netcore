using Cqrs.OrderService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Cqrs.OrderService.Infrastructure.Jobs;

public sealed class DailyOperationsReportJob(
    OrdersDbContext dbContext,
    ILogger<DailyOperationsReportJob> logger)
{
    public async Task RunAsync(CancellationToken cancellationToken)
    {
        var since = DateTimeOffset.UtcNow.AddDays(-1);

        var orderCounts = await dbContext.Orders
            .AsNoTracking()
            .Where(order => order.CreatedAt >= since)
            .GroupBy(order => order.Status)
            .Select(group => new { Status = group.Key, Count = group.Count() })
            .ToListAsync(cancellationToken);

        var pendingOutboxCount = await dbContext.OutboxMessages
            .AsNoTracking()
            .CountAsync(message => message.PublishedAt == null, cancellationToken);

        var failedOutboxCount = await dbContext.OutboxMessages
            .AsNoTracking()
            .CountAsync(
                message => message.PublishedAt == null
                    && message.PublishAttempts > 0
                    && message.LastError != null,
                cancellationToken);

        var processedAuditCount = await dbContext.IntegrationEventAudits
            .AsNoTracking()
            .CountAsync(audit => audit.ProcessedAt >= since, cancellationToken);

        logger.LogInformation(
            "Daily operations report since {Since}: orders={OrderCounts}; pendingOutbox={PendingOutboxCount}; failedOutbox={FailedOutboxCount}; processedAudits={ProcessedAuditCount}",
            since,
            string.Join(", ", orderCounts.OrderBy(item => item.Status).Select(item => $"{item.Status}={item.Count}")),
            pendingOutboxCount,
            failedOutboxCount,
            processedAuditCount);
    }
}
