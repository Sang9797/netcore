using Cqrs.OrderService.Bus.Query;

namespace Cqrs.OrderService.Application.Query;

public sealed record GetInventoryReportQuery(
    string? CategoryId,
    string? WarehouseId,
    int MinStock,
    int Page,
    int PageSize,
    IReadOnlySet<string> Fields) : IQuery<IReadOnlyList<InventoryReportItem>>
{
    public static GetInventoryReportQuery All(
        string? categoryId,
        string? warehouseId,
        int minStock,
        int page,
        int pageSize) =>
        new(categoryId, warehouseId, minStock, page, pageSize, new HashSet<string>());
}
