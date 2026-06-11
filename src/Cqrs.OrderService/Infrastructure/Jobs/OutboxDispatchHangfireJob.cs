namespace Cqrs.OrderService.Infrastructure.Jobs;

public sealed class OutboxDispatchHangfireJob(
    OutboxDispatchProcessor processor,
    ILogger<OutboxDispatchHangfireJob> logger)
{
    public async Task RunAsync(CancellationToken cancellationToken)
    {
        var processed = await processor.DispatchBatchAsync(cancellationToken);
        logger.LogInformation("Hangfire outbox dispatch processed {ProcessedCount} messages", processed);
    }
}
