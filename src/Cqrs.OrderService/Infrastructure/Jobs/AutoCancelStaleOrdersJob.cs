using Cqrs.OrderService.Application.Command;
using Cqrs.OrderService.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Cqrs.OrderService.Infrastructure.Jobs;

public sealed class AutoCancelStaleOrdersJob(
    OrdersDbContext dbContext,
    ISender sender,
    HangfireOptions options,
    ILogger<AutoCancelStaleOrdersJob> logger)
{
    public async Task RunAsync(CancellationToken cancellationToken)
    {
        var cutoff = DateTimeOffset.UtcNow.AddMinutes(-options.AutoCancelPendingOrderAfterMinutes);
        var staleOrderIds = await dbContext.Orders
            .AsNoTracking()
            .Where(order => order.Status == "PENDING" && order.CreatedAt <= cutoff)
            .OrderBy(order => order.CreatedAt)
            .Select(order => order.OrderId)
            .ToListAsync(cancellationToken);

        if (staleOrderIds.Count == 0)
        {
            logger.LogInformation("Auto-cancel job found no stale pending orders before {Cutoff}", cutoff);
            return;
        }

        var cancelledCount = 0;
        foreach (var orderId in staleOrderIds)
        {
            var result = await sender.Send(
                new CancelOrderCommand(orderId, "Cancelled automatically because the order timed out"),
                cancellationToken);

            if (result.IsSuccess)
            {
                cancelledCount += 1;
                continue;
            }

            logger.LogWarning(
                "Failed to auto-cancel stale order {OrderId}: {Errors}",
                orderId,
                string.Join("; ", result.Errors.Select(error => error.Message)));
        }

        logger.LogInformation(
            "Auto-cancel job processed {TotalCount} stale orders and cancelled {CancelledCount}",
            staleOrderIds.Count,
            cancelledCount);
    }
}
