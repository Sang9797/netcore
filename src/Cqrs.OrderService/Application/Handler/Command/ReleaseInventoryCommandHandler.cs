using Cqrs.OrderService.Application.Abstractions.Caching;
using Cqrs.OrderService.Application.Abstractions.Messaging;
using Cqrs.OrderService.Application.Command;
using Cqrs.OrderService.Application.Abstractions.Data;
using Cqrs.OrderService.Application.Common.Caching;
using Cqrs.OrderService.Application.Common.Errors;
using Cqrs.OrderService.Application.Common.Handlers;
using Cqrs.OrderService.Application.IntegrationEvents;
using Cqrs.OrderService.Domain.Model;
using FluentResults;
using MediatR;

namespace Cqrs.OrderService.Application.Handler.Command;

public sealed class ReleaseInventoryCommandHandler(
    IInventoryRepository repository,
    ICacheVersionService cacheVersionService,
    IOutboxWriter outboxWriter)
    : IRequestHandler<ReleaseInventoryCommand, Result<Unit>>
{
    public async Task<Result<Unit>> Handle(ReleaseInventoryCommand command, CancellationToken cancellationToken)
    {
        var inv = await repository.FindByProductAndWarehouse(command.ProductId, command.WarehouseId, cancellationToken);
        if (inv is null)
        {
            return Result.Fail<Unit>(ApplicationErrors.NotFound(
                "INVENTORY_NOT_FOUND",
                $"Inventory not found for product '{command.ProductId}' in warehouse '{command.WarehouseId}'"));
        }

        return await ResultHandler.Execute(async () =>
        {
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
            await cacheVersionService.IncrementVersionAsync(CacheKeys.InventoryScope, cancellationToken);
            await outboxWriter.EnqueueAsync(
                new InventoryReleasedIntegrationEvent(
                    Guid.NewGuid().ToString(),
                    DateTimeOffset.UtcNow,
                    command.ProductId,
                    command.WarehouseId,
                    command.Quantity,
                    command.OrderId),
                cancellationToken);
        });
    }
}
