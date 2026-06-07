namespace Cqrs.OrderService.Application.Query;

public sealed record InventoryReportItem(
    string ParentCategoryName,
    string CategoryName,
    string ProductId,
    string Sku,
    string ProductName,
    decimal? UnitPrice,
    string Currency,
    string WarehouseId,
    string WarehouseName,
    string Region,
    int QuantityAvailable,
    int QuantityReserved,
    int QuantityFree,
    long TotalReceived,
    long TotalShipped,
    long TransactionCount,
    DateTimeOffset? LastMovement);
