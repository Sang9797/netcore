using Cqrs.OrderService.Application.Command;
using Cqrs.OrderService.Bus.Command;
using Cqrs.OrderService.Domain.Exception;
using Cqrs.OrderService.Domain.Model;
using Cqrs.OrderService.Infrastructure.Persistence;

namespace Cqrs.OrderService.Application.Handler.Command;

public sealed class ReserveInventoryCommandHandler(IInventoryRepository repository)
    : ICommandHandler<ReserveInventoryCommand, Unit>
{
    public async Task<Unit> Handle(ReserveInventoryCommand command, CancellationToken cancellationToken)
    {
        var inv = await repository.FindByProductAndWarehouse(command.ProductId, command.WarehouseId, cancellationToken)
            ?? throw new ProductNotFoundException(command.ProductId);
        inv.Reserve(command.Quantity);
        await repository.Save(inv, cancellationToken);
        await repository.RecordTransaction(
            command.ProductId,
            command.WarehouseId,
            TransactionType.RESERVE,
            -command.Quantity,
            command.OrderId,
            $"Reserved for order {command.OrderId}",
            cancellationToken);
        return Unit.Value;
    }
}
