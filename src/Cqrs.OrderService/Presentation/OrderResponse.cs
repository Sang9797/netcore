using Cqrs.OrderService.Domain.Model;

namespace Cqrs.OrderService.Presentation;

public sealed record OrderResponse(
    string OrderId,
    string CustomerId,
    string Status,
    decimal TotalAmount,
    string Currency,
    IReadOnlyList<OrderItemResponse> Items,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt)
{
    public static OrderResponse From(Order order) =>
        new(
            order.OrderId,
            order.CustomerId,
            order.Status.ToString(),
            order.TotalAmount.Amount,
            order.TotalAmount.Currency,
            order.Items.Select(OrderItemResponse.From).ToList(),
            order.CreatedAt,
            order.UpdatedAt);
}
