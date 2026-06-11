using Cqrs.OrderService.Application.Abstractions.Caching;
using Cqrs.OrderService.Application.Abstractions.Messaging;
using Cqrs.OrderService.Application.Command;
using Cqrs.OrderService.Application.Abstractions.Data;
using Cqrs.OrderService.Application.Common.Caching;
using Cqrs.OrderService.Application.Common.Errors;
using Cqrs.OrderService.Application.Common.Handlers;
using Cqrs.OrderService.Application.IntegrationEvents;
using FluentResults;
using MediatR;

namespace Cqrs.OrderService.Application.Handler.Command;

public sealed class ConfirmOrderCommandHandler(
    IOrderRepository repository,
    ICacheVersionService cacheVersionService,
    IOutboxWriter outboxWriter)
    : IRequestHandler<ConfirmOrderCommand, Result<Unit>>
{
    public async Task<Result<Unit>> Handle(ConfirmOrderCommand command, CancellationToken cancellationToken)
    {
        var order = await repository.FindById(command.OrderId, cancellationToken);
        if (order is null)
        {
            return Result.Fail<Unit>(ApplicationErrors.NotFound(
                "ORDER_NOT_FOUND",
                $"Order '{command.OrderId}' was not found"));
        }

        return await ResultHandler.Execute(async () =>
        {
            order.Confirm();
            await repository.Save(order, cancellationToken);
            await cacheVersionService.IncrementVersionAsync(CacheKeys.OrderScope(order.OrderId), cancellationToken);
            await cacheVersionService.IncrementVersionAsync(CacheKeys.CustomerOrdersScope(order.CustomerId), cancellationToken);
            await outboxWriter.EnqueueAsync(
                new OrderConfirmedIntegrationEvent(
                    Guid.NewGuid().ToString(),
                    DateTimeOffset.UtcNow,
                    order.OrderId,
                    order.CustomerId),
                cancellationToken);
        });
    }
}
