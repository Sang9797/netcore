using Cqrs.OrderService.Application.Command;
using Cqrs.OrderService.Bus.Command;
using Cqrs.OrderService.Domain.Exception;
using Cqrs.OrderService.Domain.Model;
using Cqrs.OrderService.Infrastructure.Persistence;

namespace Cqrs.OrderService.Application.Handler.Command;

public sealed class AdjustInventoryCommandHandler(IInventoryRepository repository)
    : ICommandHandler<AdjustInventoryCommand, Unit>
{
    public async Task<Unit> Handle(AdjustInventoryCommand command, CancellationToken cancellationToken)
    {
        var inv = await repository.FindByProductAndWarehouse(command.ProductId, command.WarehouseId, cancellationToken)
            ?? throw new ProductNotFoundException(command.ProductId);
        inv.Adjust(command.Delta);
        await repository.Save(inv, cancellationToken);
        await repository.RecordTransaction(
            command.ProductId,
            command.WarehouseId,
            TransactionType.ADJUST,
            command.Delta,
            null,
            command.Reason,
            cancellationToken);
        return Unit.Value;
    }
}
