using Cqrs.OrderService.Bus.Query;
using Cqrs.OrderService.Domain.Model;

namespace Cqrs.OrderService.Application.Query;

public sealed record GetOrderByIdQuery(string OrderId) : IQuery<Order?>;
