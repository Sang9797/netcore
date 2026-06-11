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

public sealed class AdjustInventoryCommandHandler(
    IInventoryRepository repository,
    ICacheVersionService cacheVersionService,
    IOutboxWriter outboxWriter)
    : IRequestHandler<AdjustInventoryCommand, Result<Unit>>
{
    public async Task<Result<Unit>> Handle(AdjustInventoryCommand command, CancellationToken cancellationToken)
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
            await cacheVersionService.IncrementVersionAsync(CacheKeys.InventoryScope, cancellationToken);
            await outboxWriter.EnqueueAsync(
                new InventoryAdjustedIntegrationEvent(
                    Guid.NewGuid().ToString(),
                    DateTimeOffset.UtcNow,
                    command.ProductId,
                    command.WarehouseId,
                    command.Delta,
                    command.Reason),
                cancellationToken);
        });
    }
}
