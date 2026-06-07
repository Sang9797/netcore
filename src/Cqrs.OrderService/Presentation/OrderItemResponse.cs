using Cqrs.OrderService.Domain.Model;

namespace Cqrs.OrderService.Presentation;

public sealed record OrderItemResponse(
    string ItemId,
    string ProductId,
    string ProductName,
    int Quantity,
    decimal UnitPrice,
    decimal Subtotal,
    string Currency)
{
    public static OrderItemResponse From(OrderItem item) =>
        new(
            item.ItemId,
            item.ProductId,
            item.ProductName,
            item.Quantity,
            item.UnitPrice.Amount,
            item.Subtotal.Amount,
            item.UnitPrice.Currency);
}
