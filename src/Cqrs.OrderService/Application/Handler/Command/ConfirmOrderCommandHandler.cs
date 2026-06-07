using Cqrs.OrderService.Application.Command;
using Cqrs.OrderService.Bus.Command;
using Cqrs.OrderService.Domain.Exception;
using Cqrs.OrderService.Infrastructure.Persistence;

namespace Cqrs.OrderService.Application.Handler.Command;

public sealed class ConfirmOrderCommandHandler(IOrderRepository repository)
    : ICommandHandler<ConfirmOrderCommand, Unit>
{
    public async Task<Unit> Handle(ConfirmOrderCommand command, CancellationToken cancellationToken)
    {
        var order = await repository.FindById(command.OrderId, cancellationToken)
            ?? throw new OrderNotFoundException(command.OrderId);
        order.Confirm();
        await repository.Save(order, cancellationToken);
        return Unit.Value;
    }
}
