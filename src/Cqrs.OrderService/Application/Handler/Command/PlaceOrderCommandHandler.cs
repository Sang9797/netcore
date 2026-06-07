using Cqrs.OrderService.Application.Command;
using Cqrs.OrderService.Bus.Command;
using Cqrs.OrderService.Domain.Model;
using Cqrs.OrderService.Infrastructure.Persistence;

namespace Cqrs.OrderService.Application.Handler.Command;

public sealed class PlaceOrderCommandHandler(IOrderRepository repository, ILogger<PlaceOrderCommandHandler> logger)
    : ICommandHandler<PlaceOrderCommand, Order>
{
    public async Task<Order> Handle(PlaceOrderCommand command, CancellationToken cancellationToken)
    {
        logger.LogInformation("[PlaceOrder] customerId={CustomerId} items={Count}", command.CustomerId, command.Items.Count);
        var items = command.Items
            .Select(i => new OrderItem(i.ProductId, i.ProductName, i.Quantity, new Money(i.UnitPrice, i.Currency)))
            .ToList();
        return await repository.Save(Order.Create(command.CustomerId, items), cancellationToken);
    }
}
