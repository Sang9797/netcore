using Cqrs.OrderService.Domain.Exception;

namespace Cqrs.OrderService.Domain.Model;

public sealed class Inventory
{
    public Inventory(
        string inventoryId,
        string productId,
        string warehouseId,
        int quantityAvailable,
        int quantityReserved,
        DateTimeOffset lastUpdated)
    {
        InventoryId = inventoryId;
        ProductId = productId;
        WarehouseId = warehouseId;
        QuantityAvailable = quantityAvailable;
        QuantityReserved = quantityReserved;
        LastUpdated = lastUpdated;
    }

    public string InventoryId { get; }
    public string ProductId { get; }
    public string WarehouseId { get; }
    public int QuantityAvailable { get; private set; }
    public int QuantityReserved { get; private set; }
    public int QuantityFree => QuantityAvailable - QuantityReserved;
    public DateTimeOffset LastUpdated { get; private set; }

    public void Reserve(int quantity)
    {
        if (quantity <= 0)
        {
            throw new ArgumentException("Reserve quantity must be positive", nameof(quantity));
        }

        if (quantity > QuantityFree)
        {
            throw new InsufficientInventoryException(
                $"Cannot reserve {quantity} units of product {ProductId} in warehouse {WarehouseId} - only {QuantityFree} free");
        }

        QuantityReserved += quantity;
        LastUpdated = DateTimeOffset.UtcNow;
    }

    public void Release(int quantity)
    {
        if (quantity <= 0)
        {
            throw new ArgumentException("Release quantity must be positive", nameof(quantity));
        }

        QuantityReserved = Math.Max(0, QuantityReserved - quantity);
        LastUpdated = DateTimeOffset.UtcNow;
    }

    public void Adjust(int delta)
    {
        var next = QuantityAvailable + delta;
        if (next < 0)
        {
            throw new InsufficientInventoryException(
                $"Adjustment of {delta} would make stock negative for product {ProductId} in warehouse {WarehouseId}");
        }

        QuantityAvailable = next;
        LastUpdated = DateTimeOffset.UtcNow;
    }
}
