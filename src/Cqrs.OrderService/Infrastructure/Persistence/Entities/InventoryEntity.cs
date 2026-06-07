namespace Cqrs.OrderService.Infrastructure.Persistence.Entities;

public sealed class InventoryEntity
{
    public string InventoryId { get; set; } = "";
    public string ProductId { get; set; } = "";
    public string WarehouseId { get; set; } = "";
    public int QuantityAvailable { get; set; }
    public int QuantityReserved { get; set; }
    public DateTimeOffset LastUpdated { get; set; }
    public ProductEntity? Product { get; set; }
    public WarehouseEntity? Warehouse { get; set; }
}
