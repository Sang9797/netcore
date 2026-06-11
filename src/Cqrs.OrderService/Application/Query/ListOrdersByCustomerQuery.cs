using Cqrs.OrderService.Application.Abstractions.Messaging;
using Cqrs.OrderService.Domain.Model;

namespace Cqrs.OrderService.Application.Query;

public sealed record ListOrdersByCustomerQuery(string CustomerId) : IQuery<IReadOnlyList<Order>>;
