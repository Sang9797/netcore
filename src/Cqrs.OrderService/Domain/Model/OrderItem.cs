namespace Cqrs.OrderService.Domain.Model;

public sealed class OrderItem
{
    public OrderItem(string productId, string productName, int quantity, Money unitPrice)
        : this(Guid.NewGuid().ToString(), productId, productName, quantity, unitPrice)
    {
    }

    public OrderItem(string itemId, string productId, string productName, int quantity, Money unitPrice)
    {
        if (quantity <= 0)
        {
            throw new ArgumentException("Quantity must be positive", nameof(quantity));
        }

        ItemId = Required(itemId, nameof(itemId));
        ProductId = Required(productId, nameof(productId));
        ProductName = Required(productName, nameof(productName));
        Quantity = quantity;
        UnitPrice = unitPrice;
    }

    public string ItemId { get; }
    public string ProductId { get; }
    public string ProductName { get; }
    public int Quantity { get; }
    public Money UnitPrice { get; }
    public Money Subtotal => UnitPrice.Multiply(Quantity);

    private static string Required(string value, string name) =>
        string.IsNullOrWhiteSpace(value)
            ? throw new ArgumentException($"{name} required", name)
            : value;
}
