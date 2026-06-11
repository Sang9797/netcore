using Cqrs.OrderService.Infrastructure.Jobs;

namespace Cqrs.OrderService.Infrastructure.Integration;

public sealed class RabbitMqOutboxDispatcher(
    IServiceScopeFactory serviceScopeFactory,
    RabbitMqOptions options,
    ILogger<RabbitMqOutboxDispatcher> logger)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await DispatchBatch(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Failed to dispatch outbox batch");
            }

            await Task.Delay(TimeSpan.FromSeconds(options.PollIntervalSeconds), stoppingToken);
        }
    }

    private async Task DispatchBatch(CancellationToken cancellationToken)
    {
        using var scope = serviceScopeFactory.CreateScope();
        var processor = scope.ServiceProvider.GetRequiredService<OutboxDispatchProcessor>();
        await processor.DispatchBatchAsync(cancellationToken);
    }
}
