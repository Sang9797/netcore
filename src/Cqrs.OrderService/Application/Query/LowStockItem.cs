namespace Cqrs.OrderService.Application.Query;

public sealed record LowStockItem(
    string ProductId,
    string Sku,
    string ProductName,
    string WarehouseId,
    string WarehouseName,
    string Region,
    int QuantityAvailable,
    int QuantityReserved,
    int QuantityFree);
