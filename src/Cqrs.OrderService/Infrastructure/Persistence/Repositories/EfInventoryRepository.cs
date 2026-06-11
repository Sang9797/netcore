using Cqrs.OrderService.Application.Abstractions.Data;
using Cqrs.OrderService.Domain.Model;
using Cqrs.OrderService.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace Cqrs.OrderService.Infrastructure.Persistence.Repositories;

public sealed class EfInventoryRepository(OrdersDbContext dbContext) : IInventoryRepository
{
    public async Task<Inventory?> FindByProductAndWarehouse(
        string productId,
        string warehouseId,
        CancellationToken cancellationToken)
    {
        var entity = await dbContext.Inventory
            .AsNoTracking()
            .SingleOrDefaultAsync(
                i => i.ProductId == productId && i.WarehouseId == warehouseId,
                cancellationToken);
        return entity is null ? null : ToDomain(entity);
    }

    public async Task Save(Inventory inventory, CancellationToken cancellationToken)
    {
        var entity = await dbContext.Inventory
            .SingleAsync(i => i.InventoryId == inventory.InventoryId, cancellationToken);

        entity.QuantityAvailable = inventory.QuantityAvailable;
        entity.QuantityReserved = inventory.QuantityReserved;
        entity.LastUpdated = inventory.LastUpdated;
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task RecordTransaction(
        string productId,
        string warehouseId,
        TransactionType type,
        int delta,
        string? orderId,
        string? notes,
        CancellationToken cancellationToken)
    {
        dbContext.InventoryTransactions.Add(new InventoryTransactionEntity
        {
            TransactionId = Guid.NewGuid().ToString(),
            ProductId = productId,
            WarehouseId = warehouseId,
            OrderId = orderId,
            TransactionType = type.ToString(),
            QuantityDelta = delta,
            Notes = notes,
            CreatedAt = DateTimeOffset.UtcNow
        });
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static Inventory ToDomain(InventoryEntity entity) =>
        new(
            entity.InventoryId,
            entity.ProductId,
            entity.WarehouseId,
            entity.QuantityAvailable,
            entity.QuantityReserved,
            entity.LastUpdated);
}
