using Cqrs.OrderService.Application.Command;
using Cqrs.OrderService.Bus.Command;
using Cqrs.OrderService.Domain.Exception;
using Cqrs.OrderService.Infrastructure.Persistence;

namespace Cqrs.OrderService.Application.Handler.Command;

public sealed class CancelOrderCommandHandler(IOrderRepository repository)
    : ICommandHandler<CancelOrderCommand, Unit>
{
    public async Task<Unit> Handle(CancelOrderCommand command, CancellationToken cancellationToken)
    {
        var order = await repository.FindById(command.OrderId, cancellationToken)
            ?? throw new OrderNotFoundException(command.OrderId);
        order.Cancel(command.Reason);
        await repository.Save(order, cancellationToken);
        return Unit.Value;
    }
}
