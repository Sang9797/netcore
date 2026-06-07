using Cqrs.OrderService.Application.Query;
using Cqrs.OrderService.Bus.Query;
using Cqrs.OrderService.Domain.Model;
using Cqrs.OrderService.Infrastructure.Persistence;

namespace Cqrs.OrderService.Application.Handler.Query;

public sealed class ListOrdersByCustomerQueryHandler(IOrderRepository repository)
    : IQueryHandler<ListOrdersByCustomerQuery, IReadOnlyList<Order>>
{
    public Task<IReadOnlyList<Order>> Handle(ListOrdersByCustomerQuery query, CancellationToken cancellationToken) =>
        repository.FindByCustomerId(query.CustomerId, cancellationToken);
}
