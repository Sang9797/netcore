namespace Cqrs.OrderService.Infrastructure.Persistence.Entities;

public sealed class OrderItemEntity
{
    public string ItemId { get; set; } = "";
    public string OrderId { get; set; } = "";
    public string ProductId { get; set; } = "";
    public string ProductName { get; set; } = "";
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public string Currency { get; set; } = "";
    public OrderEntity? Order { get; set; }
}
