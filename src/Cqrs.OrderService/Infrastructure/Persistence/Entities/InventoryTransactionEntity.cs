namespace Cqrs.OrderService.Infrastructure.Persistence.Entities;

public sealed class InventoryTransactionEntity
{
    public string TransactionId { get; set; } = "";
    public string ProductId { get; set; } = "";
    public string WarehouseId { get; set; } = "";
    public string? OrderId { get; set; }
    public string TransactionType { get; set; } = "";
    public int QuantityDelta { get; set; }
    public string? Notes { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}
