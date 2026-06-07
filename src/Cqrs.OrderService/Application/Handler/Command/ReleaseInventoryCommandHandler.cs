using Cqrs.OrderService.Application.Command;
using Cqrs.OrderService.Bus.Command;
using Cqrs.OrderService.Domain.Exception;
using Cqrs.OrderService.Domain.Model;
using Cqrs.OrderService.Infrastructure.Persistence;

namespace Cqrs.OrderService.Application.Handler.Command;

public sealed class ReleaseInventoryCommandHandler(IInventoryRepository repository)
    : ICommandHandler<ReleaseInventoryCommand, Unit>
{
    public async Task<Unit> Handle(ReleaseInventoryCommand command, CancellationToken cancellationToken)
    {
        var inv = await repository.FindByProductAndWarehouse(command.ProductId, command.WarehouseId, cancellationToken)
            ?? throw new ProductNotFoundException(command.ProductId);
        inv.Release(command.Quantity);
        await repository.Save(inv, cancellationToken);
        await repository.RecordTransaction(
            command.ProductId,
            command.WarehouseId,
            TransactionType.RELEASE,
            command.Quantity,
            command.OrderId,
            $"Released from order {command.OrderId}",
            cancellationToken);
        return Unit.Value;
    }
}
