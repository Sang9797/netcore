using Cqrs.OrderService.Domain.Model;

namespace Cqrs.OrderService.Application.Abstractions.Data;

public interface IInventoryRepository
{
    Task<Inventory?> FindByProductAndWarehouse(string productId, string warehouseId, CancellationToken cancellationToken);

    Task Save(Inventory inventory, CancellationToken cancellationToken);

    Task RecordTransaction(
        string productId,
        string warehouseId,
        TransactionType type,
        int delta,
        string? orderId,
        string? notes,
        CancellationToken cancellationToken);
}
