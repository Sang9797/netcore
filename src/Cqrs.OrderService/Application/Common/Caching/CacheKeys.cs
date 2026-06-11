namespace Cqrs.OrderService.Application.Common.Caching;

public static class CacheKeys
{
    public const string InventoryScope = "inventory";

    public static string OrderScope(string orderId) => $"order:{orderId}";

    public static string CustomerOrdersScope(string customerId) => $"customer-orders:{customerId}";

    public static string OrderById(string orderId, long version) => $"orders:by-id:{orderId}:v{version}";

    public static string OrdersByCustomer(string customerId, long version) => $"orders:customer:{customerId}:v{version}";

    public static string InventoryReport(string? categoryId, string? warehouseId, int minStock, int page, int pageSize, long version) =>
        $"inventory:report:{categoryId ?? "all"}:{warehouseId ?? "all"}:{minStock}:{page}:{pageSize}:v{version}";

    public static string ProductInventory(string productId, long version) => $"inventory:product:{productId}:v{version}";

    public static string LowStock(int threshold, int limit, long version) => $"inventory:low-stock:{threshold}:{limit}:v{version}";
}
