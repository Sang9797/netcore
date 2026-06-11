using Cqrs.OrderService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Cqrs.OrderService.Infrastructure.Jobs;

public sealed class CleanupOldAuditRecordsJob(
    OrdersDbContext dbContext,
    HangfireOptions options,
    ILogger<CleanupOldAuditRecordsJob> logger)
{
    public async Task RunAsync(CancellationToken cancellationToken)
    {
        var cutoff = DateTimeOffset.UtcNow.AddDays(-options.AuditRetentionDays);
        var deletedCount = await dbContext.IntegrationEventAudits
            .Where(audit => audit.ProcessedAt < cutoff)
            .ExecuteDeleteAsync(cancellationToken);

        logger.LogInformation(
            "Cleanup-old-audit-records job deleted {DeletedCount} records older than {Cutoff}",
            deletedCount,
            cutoff);
    }
}
