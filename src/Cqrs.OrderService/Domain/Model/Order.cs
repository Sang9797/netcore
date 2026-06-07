using Cqrs.OrderService.Domain.Exception;

namespace Cqrs.OrderService.Domain.Model;

public sealed class Order
{
    public Order(
        string orderId,
        string customerId,
        IReadOnlyCollection<OrderItem> items,
        OrderStatus status,
        Money totalAmount,
        DateTimeOffset createdAt,
        DateTimeOffset updatedAt)
    {
        OrderId = Required(orderId, nameof(orderId));
        CustomerId = Required(customerId, nameof(customerId));
        Items = items.Count == 0
            ? throw new ArgumentException("Order must have at least one item")
            : items.ToList();
        Status = status;
        TotalAmount = totalAmount;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
    }

    public string OrderId { get; }
    public string CustomerId { get; }
    public List<OrderItem> Items { get; }
    public OrderStatus Status { get; private set; }
    public Money TotalAmount { get; private set; }
    public DateTimeOffset CreatedAt { get; }
    public DateTimeOffset UpdatedAt { get; private set; }

    public static Order Create(string customerId, IReadOnlyCollection<OrderItem> items)
    {
        if (string.IsNullOrWhiteSpace(customerId))
        {
            throw new ArgumentException("customerId is required", nameof(customerId));
        }

        if (items.Count == 0)
        {
            throw new ArgumentException("Order must have at least one item", nameof(items));
        }

        var total = items.Select(i => i.Subtotal).Aggregate(Money.Zero, (sum, money) => sum.Add(money));
        var now = DateTimeOffset.UtcNow;
        return new Order(Guid.NewGuid().ToString(), customerId, items, OrderStatus.PENDING, total, now, now);
    }

    public void Confirm()
    {
        if (Status != OrderStatus.PENDING)
        {
            throw new InvalidOrderStateException($"Only PENDING orders can be confirmed. Current: {Status}");
        }

        Status = OrderStatus.CONFIRMED;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Cancel(string reason)
    {
        if (Status is not (OrderStatus.PENDING or OrderStatus.CONFIRMED))
        {
            throw new InvalidOrderStateException($"Cannot cancel an order in status: {Status}");
        }

        Status = OrderStatus.CANCELLED;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    private static string Required(string value, string name) =>
        string.IsNullOrWhiteSpace(value)
            ? throw new ArgumentException($"{name} required", name)
            : value;
}
