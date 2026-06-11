using Cqrs.OrderService.Application.Query;

namespace Cqrs.OrderService.Application.Abstractions.Data;

public interface IInventoryReadRepository
{
    Task<IReadOnlyList<InventoryReportItem>> FindInventoryReport(
        GetInventoryReportQuery query,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<ProductStockItem>> FindProductStock(
        GetProductInventoryQuery query,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<LowStockItem>> FindLowStock(
        ListLowStockQuery query,
        CancellationToken cancellationToken);
}
