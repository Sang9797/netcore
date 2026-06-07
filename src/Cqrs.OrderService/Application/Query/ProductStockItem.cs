namespace Cqrs.OrderService.Application.Query;

public sealed record ProductStockItem(
    string ProductId,
    string Sku,
    string ProductName,
    decimal? UnitPrice,
    string Currency,
    string CategoryName,
    string WarehouseId,
    string WarehouseName,
    string Region,
    int QuantityAvailable,
    int QuantityReserved,
    int QuantityFree,
    DateTimeOffset? LastUpdated);
