using Cqrs.OrderService.Application.Abstractions.Caching;
using Cqrs.OrderService.Application.Abstractions.Messaging;
using Cqrs.OrderService.Application.Command;
using Cqrs.OrderService.Application.Abstractions.Data;
using Cqrs.OrderService.Application.Common.Caching;
using Cqrs.OrderService.Application.Common.Handlers;
using Cqrs.OrderService.Application.IntegrationEvents;
using Cqrs.OrderService.Domain.Model;
using FluentResults;
using MediatR;

namespace Cqrs.OrderService.Application.Handler.Command;

public sealed class PlaceOrderCommandHandler(
    IOrderRepository repository,
    ICacheVersionService cacheVersionService,
    IOutboxWriter outboxWriter,
    ILogger<PlaceOrderCommandHandler> logger)
    : IRequestHandler<PlaceOrderCommand, Result<Order>>
{
    public Task<Result<Order>> Handle(PlaceOrderCommand command, CancellationToken cancellationToken) =>
        ResultHandler.Execute(async () =>
        {
            logger.LogInformation("[PlaceOrder] customerId={CustomerId} items={Count}", command.CustomerId, command.Items.Count);
            var items = command.Items
                .Select(i => new OrderItem(i.ProductId, i.ProductName, i.Quantity, new Money(i.UnitPrice, i.Currency)))
                .ToList();
            var order = await repository.Save(Order.Create(command.CustomerId, items), cancellationToken);
            await cacheVersionService.IncrementVersionAsync(CacheKeys.OrderScope(order.OrderId), cancellationToken);
            await cacheVersionService.IncrementVersionAsync(CacheKeys.CustomerOrdersScope(order.CustomerId), cancellationToken);
            await outboxWriter.EnqueueAsync(
                new OrderPlacedIntegrationEvent(
                    Guid.NewGuid().ToString(),
                    DateTimeOffset.UtcNow,
                    order.OrderId,
                    order.CustomerId,
                    order.TotalAmount.Amount,
                    order.TotalAmount.Currency,
                    order.Items
                        .Select(item => new OrderPlacedItem(
                            item.ProductId,
                            item.ProductName,
                            item.Quantity,
                            item.UnitPrice.Amount,
                            item.UnitPrice.Currency))
                        .ToList()),
                cancellationToken);
            return order;
        });
}
