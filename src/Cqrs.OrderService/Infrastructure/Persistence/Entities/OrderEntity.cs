namespace Cqrs.OrderService.Infrastructure.Persistence.Entities;

public sealed class OrderEntity
{
    public string OrderId { get; set; } = "";
    public string CustomerId { get; set; } = "";
    public string Status { get; set; } = "";
    public decimal TotalAmount { get; set; }
    public string Currency { get; set; } = "";
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public List<OrderItemEntity> Items { get; set; } = [];
}
