using Cqrs.OrderService.Bus.Command;
using Cqrs.OrderService.Domain.Model;

namespace Cqrs.OrderService.Application.Command;

public sealed record PlaceOrderCommand(string CustomerId, IReadOnlyList<OrderItemCommand> Items) : ICommand<Order>;

public sealed record OrderItemCommand(
    string ProductId,
    string ProductName,
    int Quantity,
    decimal UnitPrice,
    string Currency);
